using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Outlines the entity's meshes, seen through the walls, while the component is enabled. The outline is drawn by the
/// <see cref="OutlineFeature"/> of the URP renderer; the entity's materials and meshes are left untouched.
/// </summary>
[DisallowMultipleComponent]
public class EntityOutline : MonoBehaviour
{
    private static readonly List<EntityOutline> shown = new();
    private readonly List<(Renderer renderer, int submeshCount)> meshes = new();

    public static IReadOnlyList<EntityOutline> Shown => shown;

    // Play mode starts without a domain reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => shown.Clear();

    public IReadOnlyList<(Renderer renderer, int submeshCount)> Meshes => meshes;

    private void OnEnable()
    {
        meshes.Clear();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            Mesh mesh = renderer switch
            {
                SkinnedMeshRenderer skinned => skinned.sharedMesh,
                MeshRenderer when renderer.TryGetComponent(out MeshFilter filter) => filter.sharedMesh,
                _ => null
            };
            if (mesh != null) meshes.Add((renderer, mesh.subMeshCount));
        }
        shown.Add(this);
    }

    private void OnDisable() => shown.Remove(this);
}
