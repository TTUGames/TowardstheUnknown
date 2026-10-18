using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Compiles the shader variants an edition shows before they are needed (<see cref="EditionProfile.shaderVariants"/>):
/// the decor's, the entities' and the post-processing's, which otherwise compile the first time they are seen and stall the
/// first room and the first frames after a switch. The splash warms the current edition's a few at a time, the switch
/// warms the new one's at once behind its wipe; each collection once per launch. The collections are recorded in the
/// editor (the <c>unity-playtest</c> skill's <c>record_shaders.sh</c>). On Direct3D 11, the Windows build's API, this
/// creates the shaders fully; on Metal and Vulkan it is a part of the work (their pipeline states compile on first use)
/// </summary>
public static class ShaderWarmup
{
    private static readonly HashSet<ShaderVariantCollection> warmed = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => warmed.Clear();

    /// <summary>Warms up to <paramref name="count"/> more variants of the edition's collection: true once all are</summary>
    public static bool Step(int count)
    {
        ShaderVariantCollection variants = Edition.Profile.shaderVariants;
        if (variants == null || warmed.Contains(variants)) return true;
        if (!variants.WarmUpProgressively(count)) return false;
        warmed.Add(variants);
        return true;
    }

    /// <summary>Warms the rest of the edition's collection at once</summary>
    public static void Warm()
    {
        ShaderVariantCollection variants = Edition.Profile.shaderVariants;
        if (variants == null || !warmed.Add(variants)) return;
        variants.WarmUp();
    }
}
