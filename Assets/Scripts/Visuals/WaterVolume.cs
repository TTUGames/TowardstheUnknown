using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A pool of water filling <see cref="size"/> tiles, one tile high, scaled by the transform. Builds the mesh the Water
/// shader expects: a quad per tile on top and the sides around, the vertex color marking the foam (red on the edges of
/// the box) and the UV following the local position offset by the world position, so that neighbouring pools line up.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class WaterVolume : MonoBehaviour
{
    [Tooltip("Number of tiles along X and Z")]
    [SerializeField] private Vector2Int size = new(6, 6);

    private Mesh mesh;
    private readonly List<Vector3> vertices = new(), normals = new();
    private readonly List<Vector2> uvs = new();
    private readonly List<Color> colors = new();
    private readonly List<int> triangles = new();

    // In Start rather than OnEnable: the UVs depend on the position, set by the room after instantiating it
    private void Start() => Build();

    private void OnDestroy()
    {
        if (Application.isPlaying) Destroy(mesh);
        else DestroyImmediate(mesh);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        size = Vector2Int.Max(size, Vector2Int.one);
        // A mesh can't be assigned during OnValidate
        if (mesh != null) UnityEditor.EditorApplication.delayCall += () => { if (this != null) Build(); };
    }
#endif

    private void Build()
    {
        if (mesh == null)
        {
            mesh = new Mesh { name = "Water", hideFlags = HideFlags.DontSave };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        vertices.Clear(); normals.Clear(); uvs.Clear(); colors.Clear(); triangles.Clear();

        const float bottom = -0.5f, top = 0.5f;
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
            {
                float x0 = x - 0.5f, x1 = x0 + 1, z0 = z - 0.5f, z1 = z0 + 1;
                bool negX = x == 0, posX = x == size.x - 1, negZ = z == 0, posZ = z == size.y - 1;

                // The diagonal of the corner tiles runs along their outer corner so that its foam stays symmetric
                AddQuad(Vector3.up, negX && posZ || posX && negZ,
                    (new(x0, top, z0), negX || negZ), (new(x0, top, z1), negX || posZ),
                    (new(x1, top, z1), posX || posZ), (new(x1, top, z0), posX || negZ));
                if (negX) AddSide(Vector3.left, new(x0, 0, z1), new(x0, 0, z0));
                if (posX) AddSide(Vector3.right, new(x1, 0, z0), new(x1, 0, z1));
                if (negZ) AddSide(Vector3.back, new(x0, 0, z0), new(x1, 0, z0));
                if (posZ) AddSide(Vector3.forward, new(x1, 0, z1), new(x0, 0, z1));

                void AddSide(Vector3 normal, Vector3 from, Vector3 to) => AddQuad(normal, false,
                    (from + Vector3.up * bottom, false), (from + Vector3.up * top, true),
                    (to + Vector3.up * top, true), (to + Vector3.up * bottom, false));
            }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
    }

    private void AddQuad(Vector3 normal, bool flip, params (Vector3 position, bool foam)[] corners)
    {
        int first = vertices.Count;
        foreach ((Vector3 position, bool foam) in corners)
        {
            Vector3 world = position + transform.position;
            vertices.Add(position);
            normals.Add(normal);
            uvs.Add(normal.y != 0 ? new(world.x, world.z) : normal.x != 0 ? new(world.z, world.y) : new(world.x, world.y));
            colors.Add(foam ? Color.red : Color.black);
        }
        int[] order = flip ? new[] { 1, 2, 3, 3, 0, 1 } : new[] { 0, 1, 2, 2, 3, 0 };
        foreach (int corner in order) triangles.Add(first + corner);
    }
}
