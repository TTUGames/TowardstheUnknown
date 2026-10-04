using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows the parts of the entity the scenery hides (a cliff, a fence between it and the fixed camera) as a flat
/// silhouette of its color, drawn by the <see cref="OutlineFeature"/> where its meshes are covered by something else
/// than themselves. Anniversary only: its prefabs' EditionOnly turns it off in the Classic
/// </summary>
[DisallowMultipleComponent]
public class Silhouette : MonoBehaviour
{
    private static readonly List<Silhouette> shown = new();
    private readonly List<(Renderer renderer, int submeshCount)> meshes = new();

    [SerializeField, Tooltip("Of the hidden parts, its alpha their opacity")] private Color color = new(1, 1, 1, 0.35f);

    public static IReadOnlyList<Silhouette> Shown => shown;

    // Play mode starts without a domain reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => shown.Clear();

    public IReadOnlyList<(Renderer renderer, int submeshCount)> Meshes => meshes;

    public Color Color => color;

    private void OnEnable()
    {
        meshes.Clear();
        HitFlash.CollectMeshes(gameObject, meshes);
        shown.Add(this);
    }

    private void OnDisable() => shown.Remove(this);
}
