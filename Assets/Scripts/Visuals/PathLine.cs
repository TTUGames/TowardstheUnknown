using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Draws the path the player would walk to the hovered tile in combat (<see cref="PlayerMove.PathPreviewed"/>):
/// a glowing line lying on the tiles, from the edge of the player's ring to the hovered tile, where chevrons flow
/// towards it, and a ring pulsing on the hovered tile
/// </summary>
public class PathLine : MonoBehaviour
{
    private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");

    [SerializeField, Required, Tooltip("A band with one chevron pointing along the line, repeated (texture mode Tile), twice as long as wide")] private Material material;
    [SerializeField, Required, Tooltip("A ring, laid on the hovered tile")] private Material endMaterial;
    [SerializeField, SuffixLabel("m")] private float width = 0.28f;
    [SerializeField, SuffixLabel("m"), Tooltip("Above the top of the tiles, over the selection overlays")] private float height = 0.03f;
    [SerializeField, FormerlySerializedAs("dashesPerMeter"), Min(0.1f), Tooltip("Chevrons per meter: one every two widths keeps their shape")] private float chevronsPerMeter = 1.8f;
    [SerializeField, Tooltip("Chevrons per second flowing towards the hovered tile")] private float flowSpeed = 1.2f;
    [SerializeField, SuffixLabel("m"), Tooltip("The line starts this far from the player's tile, out of its ring")] private float startGap = 0.3f;
    [SerializeField, SuffixLabel("m"), Tooltip("Of the ring on the hovered tile; the line stops at its edge")] private float endSize = 0.6f;
    [SerializeField, Range(0, 0.5f), Tooltip("The ring's pulse, a share of its size")] private float endPulse = 0.08f;
    [SerializeField, Min(0.1f), SuffixLabel("Hz")] private float endPulseSpeed = 1.5f;

    private LineRenderer line;
    private Transform end;
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
        line.numCornerVertices = 4;
        line.numCapVertices = 0;
        SetUpRenderer(line);
        line.enabled = false;

        var endObject = new GameObject("Path End", typeof(MeshFilter), typeof(MeshRenderer));
        end = endObject.transform;
        endObject.GetComponent<MeshFilter>().sharedMesh = FlatQuad();
        var endRenderer = endObject.GetComponent<MeshRenderer>();
        endRenderer.sharedMaterial = endMaterial;
        SetUpRenderer(endRenderer);
        endObject.SetActive(false);
        block = new MaterialPropertyBlock();
    }

    private static void SetUpRenderer(Renderer renderer)
    {
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }

    // A unit quad lying on the ground, facing up
    private static Mesh FlatQuad()
    {
        var mesh = new Mesh { name = "Path End Quad" };
        mesh.SetVertices(new[] { new Vector3(-0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, -0.5f) });
        mesh.SetUVs(0, new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) });
        mesh.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
        mesh.RecalculateBounds();
        return mesh;
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
        if (end != null)
        {
            Destroy(end.GetComponent<MeshFilter>().sharedMesh);
            Destroy(end.gameObject);
        }
    }

    private void Show(IReadOnlyList<Tile> path)
    {
        points.Clear();
        foreach (Tile tile in path)
            if (tile != null) points.Add(TopOf(tile) + Vector3.up * height);
        bool shown = points.Count > 1;
        end.gameObject.SetActive(shown);
        if (shown)
        {
            end.position = points[^1] + Vector3.up * 0.001f;
            // Out of the player's ring, up to the edge of the ring on the hovered tile
            points[0] = Vector3.MoveTowards(points[0], points[1], startGap);
            points[^1] = Vector3.MoveTowards(points[^1], points[^2], endSize * 0.36f);
        }
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.enabled = shown;
    }

    private void Update()
    {
        if (!line.enabled) return;
        offset -= flowSpeed * Time.deltaTime;
        offset -= Mathf.Floor(offset);
        line.GetPropertyBlock(block);
        block.SetVector(BaseMapST, new Vector4(chevronsPerMeter, 1, offset, 0));
        line.SetPropertyBlock(block);
        float pulse = 1 + endPulse * Mathf.Sin(Time.time * endPulseSpeed * 2 * Mathf.PI);
        end.localScale = Vector3.one * (endSize * pulse);
    }

    private static Vector3 TopOf(Tile tile)
    {
        Vector3 top = tile.transform.position;
        if (tile.TryGetComponent(out Collider collider)) top.y = collider.bounds.max.y;
        return top;
    }
}
