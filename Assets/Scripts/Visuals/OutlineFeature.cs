using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP renderer feature drawing the outline of the shown <see cref="EntityOutline"/>s: their silhouettes are drawn in a
/// mask, through everything, then a full-screen pass draws the outline color around the mask on the camera color.
/// It also draws the <see cref="HitFlash"/>es: the hit entities' meshes in the flash color over themselves, and the
/// <see cref="Silhouette"/>s: the entities' parts hidden by the scenery in their color (their meshes drawn through everything,
/// minus where they are seen).
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
        if (pass == null || (EntityOutline.Shown.Count == 0 && HitFlash.Shown.Count == 0 && Silhouette.Shown.Count == 0)) return;
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
        private static readonly int SilhouetteColorId = Shader.PropertyToID("_SilhouetteColor");
        private static readonly int SilhouetteSeenId = Shader.PropertyToID("_SilhouetteSeen");
        private const int MaskPass = 0, OutlinePassIndex = 1, FlashPass = 2, SilhouetteAllPass = 3, SilhouetteSeenPass = 4, SilhouettePass = 5;

        private readonly Material material;
        public float Width;

        private class PassData
        {
            public Material material;
            public TextureHandle mask;
            public TextureHandle seen;
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
                    IReadOnlyList<HitFlash> flashes = HitFlash.Shown;
                    for (int f = 0; f < flashes.Count; f++)
                    {
                        HitFlash flash = flashes[f];
                        context.cmd.SetGlobalFloat(FlashAmountId, flash.Amount);
                        IReadOnlyList<(Renderer renderer, int submeshCount)> meshes = flash.Meshes;
                        for (int m = 0; m < meshes.Count; m++)
                        {
                            (Renderer renderer, int submeshCount) = meshes[m];
                            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                            for (int submesh = 0; submesh < submeshCount; submesh++)
                                context.cmd.DrawRenderer(renderer, data.material, submesh, FlashPass);
                        }
                    }
                });
            }
            RenderTextureDescriptor descriptor = camera.cameraTargetDescriptor;
            // Its mask is tested against the scene's depth: a multisampled one (the Classic's pipeline, which shows none) is left out
            if (Silhouette.Shown.Count > 0 && descriptor.msaaSamples <= 1) RecordSilhouettes(renderGraph, resources, descriptor);
            if (EntityOutline.Shown.Count == 0) return;

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
                    IReadOnlyList<EntityOutline> outlines = EntityOutline.Shown;
                    for (int o = 0; o < outlines.Count; o++)
                    {
                        IReadOnlyList<(Renderer renderer, int submeshCount)> meshes = outlines[o].Meshes;
                        for (int m = 0; m < meshes.Count; m++)
                        {
                            (Renderer renderer, int submeshCount) = meshes[m];
                            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                            for (int submesh = 0; submesh < submeshCount; submesh++)
                                context.cmd.DrawRenderer(renderer, data.material, submesh, MaskPass);
                        }
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

        // The silhouettes' colors drawn through everything, then where they are seen (against the scene's depth), then
        // their colors where they are not seen, over the camera color
        private void RecordSilhouettes(RenderGraph renderGraph, UniversalResourceData resources, RenderTextureDescriptor descriptor)
        {
            TextureHandle all = renderGraph.CreateTexture(new TextureDesc(descriptor.width, descriptor.height)
            {
                name = "_SilhouetteAll",
                format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
                clearBuffer = true,
                clearColor = Color.clear
            });
            TextureHandle seen = renderGraph.CreateTexture(new TextureDesc(descriptor.width, descriptor.height)
            {
                name = "_SilhouetteSeen",
                format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm,
                clearBuffer = true,
                clearColor = Color.clear
            });

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Silhouettes", out PassData data))
            {
                data.material = material;
                builder.SetRenderAttachment(all, 0, AccessFlags.Write);
                // Each silhouette sets its color before drawing its meshes
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    IReadOnlyList<Silhouette> silhouettes = Silhouette.Shown;
                    for (int s = 0; s < silhouettes.Count; s++)
                    {
                        context.cmd.SetGlobalColor(SilhouetteColorId, silhouettes[s].Color);
                        DrawMeshes(context.cmd, silhouettes[s].Meshes, data.material, SilhouetteAllPass);
                    }
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Silhouettes Seen", out PassData data))
            {
                data.material = material;
                builder.SetRenderAttachment(seen, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.Read);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    IReadOnlyList<Silhouette> silhouettes = Silhouette.Shown;
                    for (int s = 0; s < silhouettes.Count; s++)
                        DrawMeshes(context.cmd, silhouettes[s].Meshes, data.material, SilhouetteSeenPass);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Silhouettes Hidden", out PassData data))
            {
                data.material = material;
                data.mask = all;
                data.seen = seen;
                builder.UseTexture(all);
                builder.UseTexture(seen);
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    context.cmd.SetGlobalTexture(SilhouetteSeenId, data.seen);
                    Blitter.BlitTexture(context.cmd, data.mask, new Vector4(1, 1, 0, 0), data.material, SilhouettePass);
                });
            }
        }

        private static void DrawMeshes(RasterCommandBuffer cmd, IReadOnlyList<(Renderer renderer, int submeshCount)> meshes, Material material, int shaderPass)
        {
            for (int m = 0; m < meshes.Count; m++)
            {
                (Renderer renderer, int submeshCount) = meshes[m];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                for (int submesh = 0; submesh < submeshCount; submesh++)
                    cmd.DrawRenderer(renderer, material, submesh, shaderPass);
            }
        }
    }
}
