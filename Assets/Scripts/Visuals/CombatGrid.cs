using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws a thin grid over the walkable tiles of the current room during the deploy phase and the combat, so that the
/// board stays readable under the ambience. One mesh per room, built on its first entry: a flat quad per tile, whose
/// shader (<c>Rendering/CombatGrid.shader</c>) draws the tile's borders. A border shared with a neighbour at the same
/// height is drawn half by each tile, the others fully by the tile, so that every line has the same width.
/// </summary>
public class CombatGrid : MonoBehaviour
{
    private static readonly int FadeId = Shader.PropertyToID("_Fade");
    private static readonly Vector3[] directions = { Vector3.left, Vector3.right, Vector3.back, Vector3.forward };

    [SerializeField, Tooltip("Rendering/CombatGrid.shader")] private Material material;
    [SerializeField, Tooltip("Height of the grid above the top of the tiles, under the selection overlays")] private float heightOffset = 0.004f;
    [SerializeField, Tooltip("Seconds to show or hide the grid")] private float fadeDuration = 0.35f;
    [SerializeField, Tooltip("Two tiles further apart in height don't share their border")] private float heightTolerance = 0.05f;

    private readonly Dictionary<Room, Mesh> meshes = new();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock block;
    private float fade;
    private bool shown;

    private void Awake()
    {
        //Built in world space, outside the Gameplay hierarchy
        var grid = new GameObject("Combat Grid");
        meshFilter = grid.AddComponent<MeshFilter>();
        meshRenderer = grid.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        meshRenderer.enabled = false;
        block = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.DeployStarted += Show;
        GameEvents.CombatStarted += Show;
        GameEvents.CombatEnded += Hide;
        GameEvents.RunEnded += OnRunEnded;
        GameEvents.RoomLeft += HideNow;
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.DeployStarted -= Show;
        GameEvents.CombatStarted -= Show;
        GameEvents.CombatEnded -= Hide;
        GameEvents.RunEnded -= OnRunEnded;
        GameEvents.RoomLeft -= HideNow;
    }

    private void OnDestroy()
    {
        foreach (Mesh mesh in meshes.Values)
            if (mesh != null) Destroy(mesh);
        if (meshFilter != null) Destroy(meshFilter.gameObject);
    }

    private void Update()
    {
        float target = shown ? 1f : 0f;
        if (fade == target) return;
        fade = Mathf.MoveTowards(fade, target, Time.deltaTime / Mathf.Max(fadeDuration, 0.001f));
        ApplyFade();
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (!meshes.TryGetValue(room, out Mesh mesh) || mesh == null)
            meshes[room] = mesh = BuildMesh(room);
        meshFilter.sharedMesh = mesh;
        HideNow();
    }

    private void Show() => shown = true;

    private void Hide() => shown = false;

    private void OnRunEnded(bool isVictory) => Hide();

    private void HideNow()
    {
        shown = false;
        fade = 0f;
        ApplyFade();
    }

    private void ApplyFade()
    {
        meshRenderer.enabled = fade > 0f && meshFilter.sharedMesh != null;
        block.SetFloat(FadeId, fade);
        meshRenderer.SetPropertyBlock(block);
    }

    /// <summary>
    /// A quad per walkable tile, on its top. UV0 spans the tile along the world X and Z axes; UV1 holds the width of
    /// its borders towards -X, +X, -Z and +Z: 1 for a border it draws alone, 0.5 for a border shared with a neighbour
    /// </summary>
    private Mesh BuildMesh(Room room)
    {
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var borders = new List<Vector4>();
        var triangles = new List<int>();

        foreach (Tile tile in room.GetComponentsInChildren<Tile>())
        {
            if (!tile.isWalkable) continue;
            float top = TopOf(tile);
            Vector3 center = tile.transform.position;
            center.y = top + heightOffset;

            var border = new Vector4();
            for (int i = 0; i < directions.Length; i++)
                border[i] = tile.lAdjacent.TryGetValue(directions[i], out Tile neighbour) && neighbour != null && neighbour.isWalkable
                    && Mathf.Abs(TopOf(neighbour) - top) <= heightTolerance ? 0.5f : 1f;

            int first = vertices.Count;
            vertices.Add(center + new Vector3(-0.5f, 0f, -0.5f));
            vertices.Add(center + new Vector3(-0.5f, 0f, 0.5f));
            vertices.Add(center + new Vector3(0.5f, 0f, 0.5f));
            vertices.Add(center + new Vector3(0.5f, 0f, -0.5f));
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(1f, 0f));
            for (int i = 0; i < 4; i++) borders.Add(border);
            triangles.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
        }

        var mesh = new Mesh { name = "Combat Grid " + room.name };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(1, borders);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    //Where the entities stand, as TacticsMove places them
    private static float TopOf(Tile tile) => tile.transform.position.y + tile.GetComponent<Collider>().bounds.extents.y;
}
