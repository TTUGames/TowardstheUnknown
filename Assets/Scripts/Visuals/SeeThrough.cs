using UnityEngine;

/// <summary>
/// Lets the hovered tile show through the decor in front of it (a cliff, a barrier, a plant): Snow Lit and Nature Lit
/// dither away what stands near the line from the tile to the game camera, nearer than the tile and above its top
/// (<c>Rendering/SeeThrough.hlsl</c>), from two global values set here on each hover, eased in and out. Nothing is done per
/// renderer: the materials stay in the SRP Batcher. Anniversary only: the Classic's materials don't read it, and this
/// clears it as it turns off
/// </summary>
public class SeeThrough : MonoBehaviour
{
    private static readonly int TileId = Shader.PropertyToID("_SeeThroughTile");
    private static readonly int ViewId = Shader.PropertyToID("_SeeThroughView");

    [SerializeField, Tooltip("The game camera: the line goes from the tile towards it")] private Camera gameCamera;
    [SerializeField, Min(0.1f), Tooltip("Radius around the line from the tile to the camera, in meters")] private float radius = 0.85f;
    [SerializeField, Range(0, 1), Tooltip("Share of the decor's pixels cut at the heart of the line")] private float strength = 0.8f;
    [SerializeField, Min(0.01f), Tooltip("Real seconds to fade in or out")] private float fadeDuration = 0.15f;

    private Vector3 tileTop;
    private float amount;
    private float target;

    private void OnEnable() => BoardPointer.TileHovered += OnTileHovered;

    private void OnDisable()
    {
        BoardPointer.TileHovered -= OnTileHovered;
        amount = target = 0;
        Shader.SetGlobalVector(TileId, Vector4.zero);
    }

    private void OnTileHovered(Tile tile)
    {
        target = tile != null ? strength : 0;
        if (tile == null) return;
        // A new tile shows at once; the fade only eases the hover's start and end
        tileTop = tile.TryGetComponent(out Collider tileCollider)
            ? new Vector3(tileCollider.bounds.center.x, tileCollider.bounds.max.y, tileCollider.bounds.center.z)
            : tile.transform.position;
        Pass();
    }

    private void Update()
    {
        if (Mathf.Approximately(amount, target)) return;
        amount = Mathf.MoveTowards(amount, target, strength * Time.unscaledDeltaTime / fadeDuration);
        Pass();
    }

    private void Pass()
    {
        Shader.SetGlobalVector(TileId, new Vector4(tileTop.x, tileTop.y, tileTop.z, amount));
        Vector3 towardsCamera = -gameCamera.transform.forward;
        Shader.SetGlobalVector(ViewId, new Vector4(towardsCamera.x, towardsCamera.y, towardsCamera.z, radius));
    }
}
