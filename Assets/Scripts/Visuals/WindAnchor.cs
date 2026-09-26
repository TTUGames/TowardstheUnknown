using UnityEngine;

/// <summary>
/// Moves the parts of a plant made of several meshes as one in the wind: passes to their materials (Nature Lit, Snow Lit)
/// the point they bend from, this transform, and the plant's height above it. With <c>hangFromTop</c>, each part is a strand
/// hanging from the top of its bounds instead, and swings from there (the wisteria). The drawing through a property block
/// leaves the SRP Batcher for these renderers only
/// </summary>
[ExecuteAlways]
public class WindAnchor : MonoBehaviour
{
    private static readonly int AnchorId = Shader.PropertyToID("_WindAnchor");

    [SerializeField, Tooltip("Each part swings from the top of its bounds, over its own length: strands hanging from a branch")] private bool hangFromTop;

    private void OnEnable() => Apply();

    private void OnDisable()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        foreach (Renderer part in GetComponentsInChildren<Renderer>(true))
        {
            part.GetPropertyBlock(block);
            block.SetVector(AnchorId, Vector4.zero);
            part.SetPropertyBlock(block);
        }
    }

    /// <summary>
    /// Passes the anchor to the parts, from their bounds where they are now: call it again if the plant moves
    /// </summary>
    public void Apply()
    {
        Renderer[] parts = GetComponentsInChildren<Renderer>(true);
        Vector3 root = transform.position;
        float height = 0.01f;
        foreach (Renderer part in parts) height = Mathf.Max(height, part.bounds.max.y - root.y);

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        foreach (Renderer part in parts)
        {
            Bounds bounds = part.bounds;
            Vector4 anchor = hangFromTop
                ? new Vector4(bounds.center.x, bounds.max.y, bounds.center.z, Mathf.Max(bounds.size.y, 0.01f))
                : new Vector4(root.x, root.y, root.z, height);
            part.GetPropertyBlock(block);
            block.SetVector(AnchorId, anchor);
            part.SetPropertyBlock(block);
        }
    }
}
