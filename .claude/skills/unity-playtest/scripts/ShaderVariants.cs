using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Records the shader variants the game draws, as an edition's collection for ShaderWarmup: a GraphicsStateCollection traced
/// in Play mode (Begin after the scene starts, End before it stops; each End adds to the recording, kept in the AppDomain
/// across the script run calls and the Play modes), then Save writes its variants as a ShaderVariantCollection, which any
/// graphics API warms (the traced states are the editor's API's). The editor's own tracking (Graphics settings) misses the
/// SRP passes of the custom shaders: it isn't used. record_shaders.sh drives it
/// </summary>
public static class ShaderVariants
{
    // The variants recorded: shader, pass, keywords. Plain values, kept in the AppDomain: a collection is an object the
    // scene changes destroy
    private static List<(Shader shader, PassIdentifier pass, string[] keywords)> Recording
    {
        get => System.AppDomain.CurrentDomain.GetData("ttu.shaderRecording") as List<(Shader, PassIdentifier, string[])>;
        set => System.AppDomain.CurrentDomain.SetData("ttu.shaderRecording", value);
    }

    private static GraphicsStateCollection Tracing
    {
        get => System.AppDomain.CurrentDomain.GetData("ttu.shaderTracing") as GraphicsStateCollection;
        set => System.AppDomain.CurrentDomain.SetData("ttu.shaderTracing", value);
    }

    public static string Clear()
    {
        Recording = new List<(Shader, PassIdentifier, string[])>();
        return "cleared";
    }

    public static string Begin()
    {
        var tracing = new GraphicsStateCollection { hideFlags = HideFlags.HideAndDontSave };
        Tracing = tracing;
        return tracing.BeginTrace() ? "tracing" : "trace refused (one already running: restart Play mode)";
    }

    public static string End()
    {
        GraphicsStateCollection tracing = Tracing;
        if (tracing == null) return "not tracing";
        tracing.EndTrace();
        var traced = new List<GraphicsStateCollection.ShaderVariant>();
        tracing.GetVariants(traced);
        Recording ??= new List<(Shader, PassIdentifier, string[])>();
        foreach (GraphicsStateCollection.ShaderVariant variant in traced)
            Recording.Add((variant.shader, variant.passId, System.Array.ConvertAll(variant.keywords, keyword => keyword.name)));
        Object.DestroyImmediate(tracing);
        Tracing = null;
        return $"traced {traced.Count} variants, {Recording.Count} recorded";
    }

    /// <summary>Saves the recorded variants at an asset path and sets them on the edition's profile ("Anniversary", "Classic")</summary>
    public static string Save(string path, string edition)
    {
        var collection = new ShaderVariantCollection();
        int skipped = 0;
        var refused = new List<string>();
        var shaders = new HashSet<Shader>();
        foreach ((Shader shader, PassIdentifier pass, string[] keywords) in Recording ?? new List<(Shader, PassIdentifier, string[])>())
        {
            // The editor's own shaders (its GUI) don't ship: a collection referencing them would bring them into the build
            string shaderPath = shader != null ? AssetDatabase.GetAssetPath(shader) : "";
            if (shader == null || shaderPath.Contains("unity editor resources") || shader.name.StartsWith("Hidden/Internal-GUI") || shader.name.StartsWith("Hidden/GUI"))
            {
                skipped++;
                continue;
            }
            // The trace names every keyword on for the draw; the collection takes those the pass declares
            var declared = new HashSet<string>();
            foreach (LocalKeyword keyword in ShaderUtil.GetPassKeywords(shader, pass)) declared.Add(keyword.name);
            string[] passKeywords = System.Array.FindAll(keywords, declared.Contains);
            try
            {
                collection.Add(new ShaderVariantCollection.ShaderVariant(shader, PassTypeOf(shader, pass), passKeywords));
                shaders.Add(shader);
            }
            catch (System.ArgumentException e)
            {
                skipped++;
                if (refused.Count < 6) refused.Add(e.Message);
            }
        }
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(collection, path);
        var profile = AssetDatabase.LoadAssetAtPath<EditionProfile>($"Assets/Data/Editions/{edition}Profile.asset");
        var serialized = new SerializedObject(profile);
        serialized.FindProperty("shaderVariants").objectReferenceValue = collection;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
        return $"saved {path}: {collection.variantCount} variants of {shaders.Count} shaders ({skipped} skipped: the editor's or refused) on {edition}Profile" + (refused.Count > 0 ? "\n  refused: " + string.Join("\n  refused: ", refused) : "");
    }

    // The legacy pass type a ShaderVariantCollection names a pass by: the URP passes are ScriptableRenderPipeline, but the
    // shadow casters, and Normal for the passes without a light mode (the post-processing's)
    private static PassType PassTypeOf(Shader shader, PassIdentifier pass)
    {
        string lightMode = shader.FindPassTagValue((int)pass.SubshaderIndex, (int)pass.PassIndex, new ShaderTagId("LightMode")).name;
        return lightMode?.ToUpperInvariant() switch
        {
            "SHADOWCASTER" => PassType.ShadowCaster,
            "META" => PassType.Meta,
            "MOTIONVECTORS" => PassType.MotionVectors,
            "SRPDEFAULTUNLIT" => PassType.ScriptableRenderPipelineDefaultUnlit,
            null or "" or "ALWAYS" => PassType.Normal,
            _ => PassType.ScriptableRenderPipeline,
        };
    }
}
