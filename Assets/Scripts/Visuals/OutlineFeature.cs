using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP renderer feature drawing the outline of the shown <see cref="EntityOutline"/>s: their silhouettes are drawn in a
/// mask, through everything, then a full-screen pass draws the outline color around the mask on the camera color.
/// It also draws the <see cref="HitFlash"/>es: the hit entities' meshes in the flash color over themselves.
/// </summary>
public class OutlineFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public Shader shader;
        public Color color = Color.white;
        [Tooltip("Outline width in pixels at 1080p, scaled with the screen height")]
        [Range(0.5f, 8f)] public float width = 2f;
        [Tooltip("Color of the hit flashes, its alpha multiplied by each flash's amount")]
        public Color flashColor = Color.white;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    [SerializeField] private Settings settings = new();
    private Material material;
    private OutlinePass pass;

    public override void Create()
    {
        if (settings.shader == null) return;
        material = CoreUtils.CreateEngineMaterial(settings.shader);
        pass = new OutlinePass(material) { renderPassEvent = settings.renderPassEvent };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null || (EntityOutline.Shown.Count == 0 && HitFlash.Shown.Count == 0)) return;
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType != CameraType.Game && cameraType != CameraType.SceneView) return;

        material.SetColor(OutlinePass.ColorId, settings.color);
        material.SetColor(OutlinePass.FlashColorId, settings.flashColor);
        pass.Width = settings.width;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing) => CoreUtils.Destroy(material);

    private class OutlinePass : ScriptableRenderPass
    {
        public static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
        public static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int StepId = Shader.PropertyToID("_OutlineStep");
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private const int MaskPass = 0, OutlinePassIndex = 1, FlashPass = 2;

        private readonly Material material;
        public float Width;

        private class PassData
        {
            public Material material;
            public TextureHandle mask;
        }

        public OutlinePass(Material material) => this.material = material;

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resources = frameData.Get<UniversalResourceData>();
            UniversalCameraData camera = frameData.Get<UniversalCameraData>();
            if (resources.isActiveTargetBackBuffer) return;

            if (HitFlash.Shown.Count > 0)
            {
                using var builder = renderGraph.AddRasterRenderPass<PassData>("Hit Flash", out PassData data);
                data.material = material;
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.Read);
                // Each flash sets its amount before drawing its meshes
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    foreach (HitFlash flash in HitFlash.Shown)
                    {
                        context.cmd.SetGlobalFloat(FlashAmountId, flash.Amount);
                        foreach ((Renderer renderer, int submeshCount) in flash.Meshes)
                        {
                            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                            for (int submesh = 0; submesh < submeshCount; submesh++)
                                context.cmd.DrawRenderer(renderer, data.material, submesh, FlashPass);
                        }
                    }
                });
            }
            if (EntityOutline.Shown.Count == 0) return;

            RenderTextureDescriptor descriptor = camera.cameraTargetDescriptor;
            TextureDesc maskDesc = new(descriptor.width, descriptor.height)
            {
                name = "_OutlineMask",
                format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm,
                clearBuffer = true,
                clearColor = Color.clear,
                filterMode = FilterMode.Bilinear
            };
            TextureHandle mask = renderGraph.CreateTexture(maskDesc);

            float pixels = Width * descriptor.height / 1080f;
            material.SetVector(StepId, new Vector4(pixels / descriptor.width, pixels / descriptor.height));

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Mask", out PassData data))
            {
                data.material = material;
                builder.SetRenderAttachment(mask, 0, AccessFlags.Write);
                builder.AllowGlobalStateModification(false);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    foreach (EntityOutline outline in EntityOutline.Shown)
                        foreach ((Renderer renderer, int submeshCount) in outline.Meshes)
                        {
                            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                            for (int submesh = 0; submesh < submeshCount; submesh++)
                                context.cmd.DrawRenderer(renderer, data.material, submesh, MaskPass);
                        }
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline", out PassData data))
            {
                data.material = material;
                data.mask = mask;
                builder.UseTexture(mask);
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                    Blitter.BlitTexture(context.cmd, data.mask, new Vector4(1, 1, 0, 0), data.material, OutlinePassIndex));
            }
        }
    }
}
