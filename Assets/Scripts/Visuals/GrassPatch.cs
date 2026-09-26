using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// A patch of grass tufts and flowers (the meshes of Tools > Nature > Generate Grass, drawn with a Nature Lit grass material)
/// placed in a room: it drops them on the ground of the decor under the patch (the large, flat faces of the rocks, cliffs and
/// cave floor whose materials it lists), never on a tile, a structure or a small stone, so that the board stays readable. Scatter, in the editor, keeps one line per tuft (its place, turn, size and mesh); the tufts are drawn
/// instanced, one draw per mesh, and move with the wind like the other plants. The decor needs colliders for Scatter to land
/// on it: add temporary ones if it has none
/// </summary>
[ExecuteAlways]
public class GrassPatch : MonoBehaviour
{
    [SerializeField, Required, Tooltip("The tufts to pick from")] private Mesh[] meshes;
    [SerializeField, Required, Tooltip("A Nature Lit material with its grass mode on, GPU instancing enabled")] private Material material;

    [BoxGroup("Scatter"), SerializeField, Min(1)] private int count = 24;
    [BoxGroup("Scatter"), SerializeField, Min(0.1f), SuffixLabel("m")] private float radius = 1.5f;
    [BoxGroup("Scatter"), SerializeField, MinMaxSlider(0.3f, 3, true)] private Vector2 scale = new Vector2(1.2f, 1.8f);
    [BoxGroup("Scatter"), SerializeField, Tooltip("The colliders the tufts land on")] private LayerMask surfaces = ~0;
    [BoxGroup("Scatter"), SerializeField, Tooltip("The materials of the ground they grow on: the rocks, the cliffs, the cave; not the structures (bridges, fences, props)")] private Material[] grounds;
    [BoxGroup("Scatter"), SerializeField, Min(0), SuffixLabel("m"), Tooltip("A surface narrower than this is a small stone: nothing grows on it")] private float minGroundSize = 0.9f;
    [BoxGroup("Scatter"), SerializeField, Range(0.5f, 1), Tooltip("How flat the ground must be: the up part of its normal")] private float flatness = 0.85f;
    [BoxGroup("Scatter"), SerializeField] private int seed = 1;
    [BoxGroup("Scatter"), SerializeField] private ShadowCastingMode shadows = ShadowCastingMode.On;

    // Local to the patch, so that the room can be placed anywhere: xyz the foot, w the turn around the up axis in degrees;
    // and x the size, y the index of the mesh
    [SerializeField, HideInInspector] private List<Vector4> spots = new List<Vector4>();
    [SerializeField, HideInInspector] private List<Vector2> looks = new List<Vector2>();

    private Matrix4x4[][] worldMatrices;
    private Matrix4x4 builtFor;
    private Bounds bounds;

    public int Count => spots.Count;

    /// <summary>
    /// Draws the placement again from the settings, on the colliders as they are now
    /// </summary>
    [BoxGroup("Scatter"), Button]
    public void Scatter()
    {
        spots.Clear();
        looks.Clear();
        if (meshes == null || meshes.Length == 0) return;
        Random.State state = Random.state;
        Random.InitState(seed);
        PhysicsScene physics = gameObject.scene.GetPhysicsScene();
        int attempts = count * 8;
        while (spots.Count < count && attempts-- > 0)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 from = transform.position + new Vector3(offset.x, 4, offset.y);
            if (!physics.Raycast(from, Vector3.down, out RaycastHit hit, 12, surfaces, QueryTriggerInteraction.Ignore)) continue;
            if (hit.collider.GetComponentInParent<EntityStats>() != null) continue;
            if (hit.normal.y < flatness || !IsGround(hit, physics)) continue;
            // Never on the board, nor on the other plants
            if (hit.collider.GetComponentInParent<Tile>() != null || hit.collider.GetComponentInParent<WindAnchor>() != null) continue;
            if (hit.collider.TryGetComponent(out Renderer surface) && surface.sharedMaterial != null && surface.sharedMaterial.IsKeywordEnabled("_WIND")) continue;
            float size = Random.Range(scale.x, scale.y);
            Vector3 foot = transform.InverseTransformPoint(hit.point);
            spots.Add(new Vector4(foot.x, foot.y, foot.z, Mathf.Round(Random.Range(0f, 360f))));
            looks.Add(new Vector2(Mathf.Round(size * 100) / 100, Random.Range(0, meshes.Length)));
        }
        Random.state = state;
        builtFor = Matrix4x4.zero;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // A large surface of one of the ground materials, flat around the point too
    private bool IsGround(RaycastHit hit, PhysicsScene physics)
    {
        Bounds size = hit.collider.bounds;
        if (Mathf.Min(size.size.x, size.size.z) < minGroundSize) return false;
        if (grounds != null && grounds.Length > 0)
        {
            if (!hit.collider.TryGetComponent(out Renderer renderer) || System.Array.IndexOf(grounds, renderer.sharedMaterial) < 0) return false;
        }
        foreach (Vector3 around in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
        {
            Vector3 from = hit.point + around * 0.2f + Vector3.up * 0.5f;
            if (!physics.Raycast(from, Vector3.down, out RaycastHit near, 1, surfaces, QueryTriggerInteraction.Ignore)) return false;
            if (near.collider != hit.collider || Mathf.Abs(near.point.y - hit.point.y) > 0.08f) return false;
        }
        return true;
    }

    private void Build()
    {
        builtFor = transform.localToWorldMatrix;
        var groups = new List<Matrix4x4>[meshes.Length];
        for (int i = 0; i < groups.Length; i++) groups[i] = new List<Matrix4x4>();
        bool first = true;
        for (int i = 0; i < spots.Count; i++)
        {
            int index = (int)looks[i].y;
            if (index < 0 || index >= meshes.Length) continue;
            Vector4 spot = spots[i];
            Matrix4x4 world = builtFor * Matrix4x4.TRS(spot, Quaternion.Euler(0, spot.w, 0), Vector3.one * looks[i].x);
            groups[index].Add(world);
            Vector3 point = world.GetColumn(3);
            if (first) bounds = new Bounds(point, Vector3.one);
            else bounds.Encapsulate(point);
            first = false;
        }
        bounds.Expand(1);
        worldMatrices = new Matrix4x4[meshes.Length][];
        for (int i = 0; i < groups.Length; i++) worldMatrices[i] = groups[i].ToArray();
    }

    private void Update()
    {
        if (material == null || meshes == null || meshes.Length == 0 || spots.Count == 0) return;
        if (worldMatrices == null || worldMatrices.Length != meshes.Length || transform.localToWorldMatrix != builtFor) Build();
        var parameters = new RenderParams(material)
        {
            worldBounds = bounds,
            shadowCastingMode = shadows,
            receiveShadows = true,
            layer = gameObject.layer,
        };
        for (int i = 0; i < meshes.Length; i++)
            if (meshes[i] != null && worldMatrices[i].Length > 0)
                Graphics.RenderMeshInstanced(parameters, meshes[i], 0, worldMatrices[i]);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.9f, 0.5f, 0.6f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, new Vector3(1, 0.02f, 1));
        Gizmos.DrawWireSphere(Vector3.zero, radius);
    }
}
