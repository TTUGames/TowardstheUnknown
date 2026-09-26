using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The planar reflection of the water: each frame a pool is on screen, a camera placed as the mirror image of this one
/// under the water plane (the height of the largest visible <see cref="WaterSurface"/>) draws the room into a texture
/// that Water.shader reads (_WaterReflectionTex, _WaterReflectionPlane). The pools at another height read it shifted.
/// The mirror camera is a proper camera looking up from below: its image is the mirror image flipped left to right,
/// which the shader undoes, so that the faces keep their winding
/// </summary>
[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(10000)]
public class WaterReflection : MonoBehaviour
{
    private static readonly int TextureId = Shader.PropertyToID("_WaterReflectionTex");
    private static readonly int PlaneId = Shader.PropertyToID("_WaterReflectionPlane");

    [SerializeField, Tooltip("What the water reflects: never the Water layer itself")] private LayerMask cullingMask = ~0;
    [SerializeField, Range(0.25f, 1), Tooltip("Of the camera's resolution")] private float resolution = 0.33f;
    [SerializeField, SuffixLabel("m"), Tooltip("Lifts the clip plane over the water, against the seams where the ground meets it")] private float clipOffset = 0.03f;
    [SerializeField, Tooltip("The reflection casts the lights' shadows: about 0.2 ms more at 4K")] private bool shadows;

    private Camera source;
    private Camera mirror;
    private RenderTexture texture;

    private void OnEnable()
    {
        source = GetComponent<Camera>();
        Shader.SetGlobalVector(PlaneId, Vector4.zero);
    }

    private void OnDisable()
    {
        Shader.SetGlobalVector(PlaneId, Vector4.zero);
        Shader.SetGlobalTexture(TextureId, Texture2D.blackTexture);
        if (texture != null)
        {
            texture.Release();
            Destroy(texture);
            texture = null;
        }
        if (mirror != null) Destroy(mirror.gameObject);
    }

    private void LateUpdate()
    {
        WaterSurface pool = LargestVisiblePool();
        if (pool == null)
        {
            Shader.SetGlobalVector(PlaneId, Vector4.zero);
            return;
        }
        float height = pool.Height;
        PrepareMirror();
        PlaceMirror(height);

        var request = new UniversalRenderPipeline.SingleCameraRequest { destination = texture };
        if (!RenderPipeline.SupportsRenderRequest(mirror, request)) return;
        RenderPipeline.SubmitRenderRequest(mirror, request);
        Shader.SetGlobalTexture(TextureId, texture);
        Shader.SetGlobalVector(PlaneId, new Vector4(height, 1, 0, 0));
    }

    private static WaterSurface LargestVisiblePool()
    {
        WaterSurface largest = null;
        foreach (WaterSurface pool in WaterSurface.Active)
            if (pool.IsVisible && (largest == null || pool.Area > largest.Area)) largest = pool;
        return largest;
    }

    private void PrepareMirror()
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(source.pixelWidth * resolution));
        int height = Mathf.Max(1, Mathf.RoundToInt(source.pixelHeight * resolution));
        if (texture == null || texture.width != width || texture.height != height)
        {
            if (texture != null)
            {
                texture.Release();
                Destroy(texture);
            }
            // Mipmapped: the ripples blur the reflection by reading lower mips
            texture = new RenderTexture(width, height, 24, RenderTextureFormat.DefaultHDR)
            {
                name = "Water Reflection",
                useMipMap = true,
                autoGenerateMips = true,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Trilinear,
                hideFlags = HideFlags.DontSave,
            };
        }

        if (mirror != null) return;
        var holder = new GameObject("Water Reflection Camera") { hideFlags = HideFlags.HideAndDontSave };
        mirror = holder.AddComponent<Camera>();
        mirror.enabled = false;
        UniversalAdditionalCameraData data = mirror.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
        data.renderShadows = shadows;
        data.requiresColorOption = CameraOverrideOption.Off;
        data.requiresDepthOption = CameraOverrideOption.Off;
        data.antialiasing = AntialiasingMode.None;
    }

    private void PlaceMirror(float height)
    {
        mirror.CopyFrom(source);
        mirror.enabled = false;
        mirror.rect = new Rect(0, 0, 1, 1);
        mirror.targetTexture = texture;
        mirror.cullingMask = cullingMask;

        // The mirror image of the camera under the plane: its position and axes reflected
        Transform view = source.transform;
        Vector3 position = view.position;
        position.y = 2 * height - position.y;
        Vector3 forward = view.forward, up = view.up;
        forward.y = -forward.y;
        up.y = -up.y;
        mirror.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, up));

        // Nothing under the water: the near plane follows the water plane
        mirror.ResetProjectionMatrix();
        Matrix4x4 worldToCamera = mirror.worldToCameraMatrix;
        Vector3 normal = worldToCamera.MultiplyVector(Vector3.up).normalized;
        Vector3 point = worldToCamera.MultiplyPoint(new Vector3(0, height + clipOffset, 0));
        mirror.projectionMatrix = mirror.CalculateObliqueMatrix(new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, point)));
    }
}
