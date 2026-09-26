using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Draws the path the player would walk to the hovered tile in combat (<see cref="PlayerMove.PathPreviewed"/>):
/// a line of dashes lying on the tiles, from the player's tile to the hovered one, the dashes flowing towards it
/// </summary>
public class PathLine : MonoBehaviour
{
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");

    [SerializeField, Required, Tooltip("A dash texture repeated along the line (texture mode Tile)")] private Material material;
    [SerializeField, SuffixLabel("m")] private float width = 0.15f;
    [SerializeField, SuffixLabel("m"), Tooltip("Above the top of the tiles, over the selection overlays")] private float height = 0.03f;
    [SerializeField, Min(0.1f), Tooltip("Dashes per meter")] private float dashesPerMeter = 3f;
    [SerializeField, Tooltip("Dashes per second flowing towards the hovered tile")] private float flowSpeed = 1.5f;

    private LineRenderer line;
    private MaterialPropertyBlock block;
    private PlayerMove player;
    private readonly List<Vector3> points = new();
    private float offset;

    private void Awake()
    {
        //Built in world space, outside the Gameplay hierarchy, lying flat: the quads face up
        var lineObject = new GameObject("Path Line");
        lineObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        line = lineObject.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.useWorldSpace = true;
        line.alignment = LineAlignment.TransformZ;
        line.textureMode = LineTextureMode.Tile;
        line.widthMultiplier = width;
        line.numCornerVertices = 3;
        line.numCapVertices = 2;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        line.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        line.enabled = false;
        block = new MaterialPropertyBlock();
    }

    // The player's modes are ready once every Awake ran
    private void Start()
    {
        player = GameScene.Player != null ? GameScene.Player.playerMove : null;
        if (player != null) player.PathPreviewed += Show;
    }

    private void OnDestroy()
    {
        if (player != null) player.PathPreviewed -= Show;
        if (line != null) Destroy(line.gameObject);
    }

    private void Show(IReadOnlyList<Tile> path)
    {
        points.Clear();
        foreach (Tile tile in path)
            if (tile != null) points.Add(TopOf(tile) + Vector3.up * height);
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.enabled = points.Count > 1;
    }

    private void Update()
    {
        if (!line.enabled) return;
        offset -= flowSpeed * Time.deltaTime;
        offset -= Mathf.Floor(offset);
        line.GetPropertyBlock(block);
        block.SetVector(BaseMapST, new Vector4(dashesPerMeter, 1, offset, 0));
        line.SetPropertyBlock(block);
    }

    private static Vector3 TopOf(Tile tile)
    {
        Vector3 top = tile.transform.position;
        if (tile.TryGetComponent(out Collider collider)) top.y = collider.bounds.max.y;
        return top;
    }
}
