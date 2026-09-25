using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Tells the Snow Lit surfaces where the sky is open: when a room is entered, draws the height of its meshes seen from the top
/// (keeping the highest) into a global height map. A surface lower than the map is sheltered and gets no snow
/// </summary>
public class SnowCover : MonoBehaviour
{
    private static readonly int HeightMapId = Shader.PropertyToID("_SnowHeightMap");
    private static readonly int BoundsId = Shader.PropertyToID("_SnowHeightBounds");
    private static readonly int BiasId = Shader.PropertyToID("_SnowHeightBias");
    private const string SnowShaderName = "Towards the Unknown/Snow Lit";

    [SerializeField, Tooltip("Hidden/Snow Height")] private Shader heightShader;
    [SerializeField, Tooltip("Texels on each side of the map")] private int resolution = 256;
    [SerializeField, Tooltip("Around the room's meshes, in meters")] private float margin = 2;
    [SerializeField, Tooltip("A surface this far under the highest point above it still counts as open, in meters")] private float bias = 0.12f;

    private Material heightMaterial;
    private RenderTexture heightMap;

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        Shader.SetGlobalVector(BoundsId, Vector4.zero);
    }

    private void OnDestroy()
    {
        if (heightMap != null) heightMap.Release();
        if (heightMaterial != null) Destroy(heightMaterial);
    }

    private void OnRoomEntered(Room room, bool firstVisit) => Build(room.gameObject);

    /// <summary>
    /// Draws the height map of the opaque, static meshes under the root
    /// </summary>
    public void Build(GameObject root)
    {
        if (heightShader == null) return;
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();
        Bounds bounds = new Bounds();
        bool any = false;
        foreach (MeshRenderer renderer in renderers)
        {
            if (!IsOccluder(renderer)) continue;
            if (!any) bounds = renderer.bounds;
            else bounds.Encapsulate(renderer.bounds);
            any = true;
        }
        if (!any) return;

        // A square around the room, the map's texels being square too
        float size = Mathf.Max(bounds.size.x, bounds.size.z) + margin * 2;
        Vector4 area = new Vector4(bounds.center.x - size / 2, bounds.center.z - size / 2, size, 1);

        if (heightMaterial == null) heightMaterial = new Material(heightShader);
        if (heightMap == null)
        {
            heightMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RHalf) { name = "Snow Height Map", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            heightMap.Create();
        }

        var commands = new CommandBuffer { name = "Snow Height Map" };
        commands.SetRenderTarget(heightMap);
        commands.ClearRenderTarget(false, true, new Color(-1000, 0, 0, 0));
        commands.SetGlobalVector(BoundsId, area);
        foreach (MeshRenderer renderer in renderers)
        {
            if (!IsOccluder(renderer)) continue;
            int submeshes = renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount;
            for (int submesh = 0; submesh < submeshes; submesh++)
                commands.DrawRenderer(renderer, heightMaterial, submesh, 0);
        }
        Graphics.ExecuteCommandBuffer(commands);
        commands.Release();

        Shader.SetGlobalTexture(HeightMapId, heightMap);
        Shader.SetGlobalVector(BoundsId, area);
        Shader.SetGlobalFloat(BiasId, bias);
    }

    // The visible, static meshes under snow: not the hidden overlays nor the entities, which move
    private static bool IsOccluder(MeshRenderer renderer)
    {
        if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) return false;
        if (renderer.GetComponentInParent<EntityStats>() != null) return false;
        if (!renderer.TryGetComponent(out MeshFilter filter) || filter.sharedMesh == null) return false;
        // Only the snowy ground, rocks and props shelter: thin plants and vines would leave hard shadows of bare ground
        Material material = renderer.sharedMaterial;
        return material != null && material.shader != null && material.shader.name == SnowShaderName;
    }
}
