using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates the low poly grass tufts and flower clumps drawn by <see cref="GrassPatch"/> with the Nature Lit grass
/// materials, into Art/Models/Nature/Grass. Each blade is a tapered, curved strip seen from both sides; the vertex color
/// holds its height along the blade (r, the wind's weight and the color's gradient), a tint per blade (g) and the petals (b).
/// Run it again after changing a shape: the meshes are replaced in place, keeping their references
/// </summary>
public static class GrassMeshGenerator
{
    private const string Folder = "Assets/Art/Models/Nature/Grass";
    private const int Segments = 3;

    private struct Tuft
    {
        public string name;
        public int seed;
        public int blades;
        public float radius;
        public Vector2 height;
        public Vector2 width;
        public float lean;
        public int flowers;
        public Vector2 flowerHeight;
    }

    private static readonly Tuft[] Tufts =
    {
        new Tuft { name = "Grass_Tuft_01", seed = 11, blades = 9, radius = 0.09f, height = new Vector2(0.126f, 0.21f), width = new Vector2(0.03f, 0.045f), lean = 0.35f },
        new Tuft { name = "Grass_Tuft_02", seed = 23, blades = 14, radius = 0.13f, height = new Vector2(0.154f, 0.266f), width = new Vector2(0.03f, 0.05f), lean = 0.4f },
        new Tuft { name = "Grass_Tuft_03", seed = 37, blades = 6, radius = 0.06f, height = new Vector2(0.063f, 0.119f), width = new Vector2(0.025f, 0.035f), lean = 0.3f },
        new Tuft { name = "Grass_Tuft_04", seed = 41, blades = 18, radius = 0.2f, height = new Vector2(0.105f, 0.21f), width = new Vector2(0.03f, 0.045f), lean = 0.45f },
        new Tuft { name = "Grass_Flowers_01", seed = 53, blades = 8, radius = 0.1f, height = new Vector2(0.112f, 0.182f), width = new Vector2(0.03f, 0.04f), lean = 0.35f, flowers = 2, flowerHeight = new Vector2(0.21f, 0.28f) },
        new Tuft { name = "Grass_Flowers_02", seed = 67, blades = 5, radius = 0.08f, height = new Vector2(0.084f, 0.14f), width = new Vector2(0.025f, 0.035f), lean = 0.3f, flowers = 3, flowerHeight = new Vector2(0.14f, 0.21f) },
    };

    [MenuItem("Tools/Nature/Generate Grass")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art/Models/Nature", "Grass");
        foreach (Tuft tuft in Tufts)
        {
            Mesh mesh = Build(tuft);
            string path = $"{Folder}/{tuft.name}.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing);
                EditorUtility.SetDirty(existing);
            }
            else AssetDatabase.CreateAsset(mesh, path);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Generated {Tufts.Length} grass meshes in {Folder}");
    }

    private static Mesh Build(Tuft tuft)
    {
        Random.State state = Random.state;
        Random.InitState(tuft.seed);
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var colors = new List<Color>();
        var triangles = new List<int>();

        for (int i = 0; i < tuft.blades; i++)
        {
            Vector2 foot = Random.insideUnitCircle * tuft.radius;
            // The blades lean outwards from the middle of the tuft, a little
            Vector3 outwards = foot.sqrMagnitude > 1e-6f ? new Vector3(foot.x, 0, foot.y).normalized : Random.onUnitSphere;
            outwards.y = 0;
            float height = Random.Range(tuft.height.x, tuft.height.y);
            Blade(vertices, normals, colors, triangles, new Vector3(foot.x, 0, foot.y), outwards.normalized, height,
                  Random.Range(tuft.width.x, tuft.width.y), Random.Range(0.5f, 1f) * tuft.lean * height, Random.value, 0);
        }
        for (int i = 0; i < tuft.flowers; i++)
        {
            Vector2 foot = Random.insideUnitCircle * tuft.radius * 0.6f;
            Vector3 lean = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            float height = Random.Range(tuft.flowerHeight.x, tuft.flowerHeight.y);
            float bend = 0.15f * height;
            Vector3 top = Blade(vertices, normals, colors, triangles, new Vector3(foot.x, 0, foot.y), lean, height, 0.012f, bend, Random.value, 0);
            Flower(vertices, normals, colors, triangles, top, Random.Range(0.03f, 0.045f), Random.value);
        }

        Random.state = state;
        var mesh = new Mesh { name = tuft.name };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    // A tapered strip from the foot to a single tip vertex, curving along lean, both faces. Returns its tip
    private static Vector3 Blade(List<Vector3> vertices, List<Vector3> normals, List<Color> colors, List<int> triangles,
                                 Vector3 foot, Vector3 lean, float height, float width, float bend, float tint, float petal)
    {
        // Seen flat on from the side it leans to, turned a little
        Vector3 right = Quaternion.Euler(0, Random.Range(-35f, 35f), 0) * Vector3.Cross(Vector3.up, lean).normalized;
        Vector3 face = Vector3.Cross(right, Vector3.up).normalized;
        for (int side = 0; side < 2; side++)
        {
            int start = vertices.Count;
            // Lit mostly as the ground under them, so that they don't flicker from dark to light as they turn
            Vector3 normal = Vector3.Lerp(side == 0 ? face : -face, Vector3.up, 0.65f).normalized;
            for (int level = 0; level <= Segments; level++)
            {
                float t = level / (float)Segments;
                Vector3 center = foot + Vector3.up * (height * t) + lean * (bend * t * t);
                if (level == Segments)
                {
                    vertices.Add(center);
                    normals.Add(normal);
                    colors.Add(new Color(1, tint, petal, 1));
                    break;
                }
                float half = width * 0.5f * Mathf.Pow(1 - t, 0.8f);
                vertices.Add(center - right * half);
                vertices.Add(center + right * half);
                normals.Add(normal);
                normals.Add(normal);
                colors.Add(new Color(t, tint, petal, 1));
                colors.Add(new Color(t, tint, petal, 1));
            }
            for (int level = 0; level < Segments; level++)
            {
                int a = start + level * 2;
                int tip = start + Segments * 2;
                int[] quad = level < Segments - 1 ? new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 } : new[] { a, tip, a + 1 };
                // Clockwise seen from the side the normal faces
                if (side == 0) System.Array.Reverse(quad);
                triangles.AddRange(quad);
            }
        }
        return foot + Vector3.up * height + lean * bend;
    }

    // Five petals around the top of a stalk, open to the sky, seen from both sides
    private static void Flower(List<Vector3> vertices, List<Vector3> normals, List<Color> colors, List<int> triangles, Vector3 center, float size, float tint)
    {
        float turn = Random.Range(0f, 72f);
        for (int side = 0; side < 2; side++)
        {
            Vector3 normal = side == 0 ? Vector3.up : Vector3.down;
            int middle = vertices.Count;
            vertices.Add(center + Vector3.up * 0.005f);
            normals.Add(normal);
            colors.Add(new Color(1, tint, 0.6f, 1));
            for (int petal = 0; petal < 5; petal++)
            {
                Quaternion around = Quaternion.Euler(0, turn + petal * 72, 0);
                Vector3 tip = center + around * new Vector3(0, size * 0.25f, size);
                Vector3 left = center + around * new Vector3(-size * 0.35f, size * 0.1f, size * 0.55f);
                Vector3 right = center + around * new Vector3(size * 0.35f, size * 0.1f, size * 0.55f);
                int first = vertices.Count;
                vertices.Add(left);
                vertices.Add(tip);
                vertices.Add(right);
                for (int k = 0; k < 3; k++)
                {
                    normals.Add(normal);
                    colors.Add(new Color(1, tint, 1, 1));
                }
                int[] fan = { middle, first, first + 1, middle, first + 1, first + 2 };
                if (side == 1) System.Array.Reverse(fan);
                triangles.AddRange(fan);
            }
        }
    }
}
