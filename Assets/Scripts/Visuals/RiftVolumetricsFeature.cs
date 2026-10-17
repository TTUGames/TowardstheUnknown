using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP renderer feature drawing the rift's air (the Rift Volumetrics shader, light mode RiftVolumetrics) at a fraction of the
/// screen's resolution, then laying it over the camera color with a depth aware upsample: the ray march is the Anniversary's
/// costliest pass, and soft air hardly shows the lower resolution
/// </summary>
public class RiftVolumetricsFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public Shader compositeShader;
        [Range(0.25f, 1f), Tooltip("The air's resolution, as a share of the screen's on each side")]
        public float scale = 0.5f;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    }

    [SerializeField] private Settings settings = new();
    private Material material;
    private RiftVolumetricsPass pass;

    public override void Create()
    {
        if (settings.compositeShader == null) return;
        material = CoreUtils.CreateEngineMaterial(settings.compositeShader);
        pass = new RiftVolumetricsPass(material) { renderPassEvent = settings.renderPassEvent };
        pass.ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null) return;
        CameraType cameraType = renderingData.cameraData.cameraType;
        // Not in the water's mirror camera: its reflection gets no air
        if (cameraType != CameraType.Game && cameraType != CameraType.SceneView) return;
        pass.Scale = settings.scale;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing) => CoreUtils.Destroy(material);

    private class RiftVolumetricsPass : ScriptableRenderPass
    {
        private static readonly ShaderTagId LightMode = new("RiftVolumetrics");
        private static readonly int TargetSizeId = Shader.PropertyToID("_RiftTargetSize");

        private readonly Material material;
        public float Scale;

        private class DrawData
        {
            public RendererListHandle list;
            public Vector4 size;
        }

        private class CompositeData
        {
            public Material material;
            public TextureHandle air;
            public Vector4 size;
        }

        public RiftVolumetricsPass(Material material)
        {
            this.material = material;
            profilingSampler = new ProfilingSampler("Rift Volumetrics");
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resources = frameData.Get<UniversalResourceData>();
            UniversalCameraData camera = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            if (resources.isActiveTargetBackBuffer) return;

            RenderTextureDescriptor descriptor = camera.cameraTargetDescriptor;
            int width = Mathf.Max(1, Mathf.RoundToInt(descriptor.width * Scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(descriptor.height * Scale));
            Vector4 size = new(width, height, 1f / width, 1f / height);
            // No light and the whole scene showing through, where the air isn't drawn
            TextureHandle air = renderGraph.CreateTexture(new TextureDesc(width, height)
            {
                name = "_RiftAir",
                format = GraphicsFormat.R16G16B16A16_SFloat,
                clearBuffer = true,
                clearColor = new Color(0, 0, 0, 1),
                filterMode = FilterMode.Point
            });

            using (var builder = renderGraph.AddRasterRenderPass<DrawData>("Rift Air", out DrawData data))
            {
                var drawing = new DrawingSettings(LightMode, new SortingSettings(camera.camera) { criteria = SortingCriteria.None });
                data.list = renderGraph.CreateRendererList(new RendererListParams(renderingData.cullResults, drawing, new FilteringSettings(RenderQueueRange.all)));
                data.size = size;
                builder.UseRendererList(data.list);
                builder.SetRenderAttachment(air, 0, AccessFlags.Write);
                // The air reads the lights' shadows, cookies and clusters, and the scene's depth
                if (resources.mainShadowsTexture.IsValid()) builder.UseTexture(resources.mainShadowsTexture, AccessFlags.Read);
                if (resources.additionalShadowsTexture.IsValid()) builder.UseTexture(resources.additionalShadowsTexture, AccessFlags.Read);
                if (resources.cameraDepthTexture.IsValid()) builder.UseTexture(resources.cameraDepthTexture, AccessFlags.Read);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (DrawData data, RasterGraphContext context) =>
                {
                    context.cmd.SetGlobalVector(TargetSizeId, data.size);
                    context.cmd.DrawRendererList(data.list);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<CompositeData>("Rift Air Composite", out CompositeData data))
            {
                data.material = material;
                data.air = air;
                data.size = size;
                builder.UseTexture(air);
                if (resources.cameraDepthTexture.IsValid()) builder.UseTexture(resources.cameraDepthTexture, AccessFlags.Read);
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (CompositeData data, RasterGraphContext context) =>
                {
                    context.cmd.SetGlobalVector(TargetSizeId, data.size);
                    Blitter.BlitTexture(context.cmd, data.air, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }
    }
}
