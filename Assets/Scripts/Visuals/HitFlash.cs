using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The meshes of an entity flashing white when hit, drawn over it by the <see cref="OutlineFeature"/>:
/// its materials are left untouched. <c>EntityFeedback</c> starts and fades the flash
/// </summary>
public class HitFlash
{
    private static readonly List<HitFlash> shown = new();

    public static IReadOnlyList<HitFlash> Shown => shown;

    // Play mode starts without a domain reload: a flash shown when the last session stopped would stay listed
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => shown.Clear();

    private readonly List<(Renderer renderer, int submeshCount)> meshes = new();

    public IReadOnlyList<(Renderer renderer, int submeshCount)> Meshes => meshes;

    /// <summary>
    /// The flash's opacity, from 0 to 1
    /// </summary>
    public float Amount { get; private set; }

    public HitFlash(GameObject entity) => CollectMeshes(entity, meshes);

    /// <summary>
    /// The mesh renderers under the root, with their submesh counts: what the <see cref="OutlineFeature"/> draws
    /// </summary>
    public static void CollectMeshes(GameObject root, List<(Renderer renderer, int submeshCount)> meshes)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Mesh mesh = renderer switch
            {
                SkinnedMeshRenderer skinned => skinned.sharedMesh,
                MeshRenderer when renderer.TryGetComponent(out MeshFilter filter) => filter.sharedMesh,
                _ => null
            };
            if (mesh != null) meshes.Add((renderer, mesh.subMeshCount));
        }
    }

    /// <summary>
    /// Shows the flash at this opacity, hides it at 0
    /// </summary>
    public void Set(float amount)
    {
        Amount = Mathf.Clamp01(amount);
        if (Amount > 0 && !shown.Contains(this)) shown.Add(this);
        else if (Amount <= 0) shown.Remove(this);
    }
}
