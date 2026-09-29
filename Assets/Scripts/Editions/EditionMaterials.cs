using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gives the renderers the materials of the current edition, from the pairs of the <see cref="EditionSkin"/>.
/// Applied to each scene as it loads (<see cref="Edition"/>), to what code instantiates (the rooms, the enemies, the pooled VFX),
/// and to every loaded scene when the edition changes, inactive objects included (the rooms left, the pool).
/// A renderer given its Classic materials keeps its Anniversary ones, to get them back
/// </summary>
public static class EditionMaterials
{
    private static readonly Dictionary<Renderer, Material[]> anniversary = new();
    private static readonly List<Material> buffer = new();
    private static readonly List<Renderer> renderers = new();
    private static readonly List<GameObject> roots = new();
    private static readonly List<Renderer> destroyed = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => anniversary.Clear();

    /// <summary>
    /// Gives every renderer under <paramref name="root"/> (particles included) the materials of the current edition
    /// </summary>
    public static void Apply(GameObject root)
    {
        EditionSkin skin = GameAssets.Instance.classicSkin;
        if (root == null || skin == null || !skin.HasMaterials) return;
        bool classic = Edition.IsClassic;
        // In the Anniversary, only the renderers given their Classic materials have something to get back
        if (!classic && anniversary.Count == 0) return;
        root.GetComponentsInChildren(true, renderers);
        foreach (Renderer renderer in renderers)
        {
            if (!classic)
            {
                if (anniversary.Remove(renderer, out Material[] materials)) renderer.sharedMaterials = materials;
                continue;
            }
            if (anniversary.ContainsKey(renderer)) continue;
            renderer.GetSharedMaterials(buffer);
            bool changed = false;
            for (int i = 0; i < buffer.Count; i++)
            {
                Material material = skin.Classic(buffer[i]);
                if (material == buffer[i]) continue;
                if (!changed) anniversary[renderer] = buffer.ToArray();
                buffer[i] = material;
                changed = true;
            }
            if (changed) renderer.SetSharedMaterials(buffer);
        }
    }

    public static void Apply(Scene scene)
    {
        ForgetDestroyed();
        scene.GetRootGameObjects(roots);
        foreach (GameObject root in roots) Apply(root);
    }

    public static void ApplyToLoadedScenes()
    {
        ForgetDestroyed();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded) Apply(scene);
        }
    }

    private static void ForgetDestroyed()
    {
        destroyed.Clear();
        foreach (Renderer renderer in anniversary.Keys)
            if (renderer == null) destroyed.Add(renderer);
        foreach (Renderer renderer in destroyed) anniversary.Remove(renderer);
    }
}
