using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

/// <summary>
/// Plays VFX once out of sight before they are needed: their instances fill the <c>VFXPool</c>, and rendering them with a hidden camera
/// compiles their shaders, which would otherwise freeze the game the first time an effect shows.
/// Called when a room is entered (its enemies' patterns and the player's artifacts), when a chest is filled and when the inventory changes
/// </summary>
public static class VFXWarmup
{
    //Far from any room, seen by the warmup camera only
    private static readonly Vector3 stagePosition = new Vector3(0, -1000, 0);

    private static readonly HashSet<GameObject> warmed = new HashSet<GameObject>();
    private static Camera camera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        warmed.Clear();
        camera = null;
    }

    public static void Warm(IEnumerable<AbilityData> abilities) => Warm(abilities.Where(ability => ability != null).SelectMany(ability => ability.VFXPrefabs));

    public static void Warm(IEnumerable<Ability> abilities) => Warm(abilities.SelectMany(ability => ability.VFXPrefabs));

    /// <summary>
    /// Instantiates and renders once the prefabs not warmed yet in this scene
    /// </summary>
    public static void Warm(IEnumerable<GameObject> prefabs)
    {
        //The camera dies with the scene, and the pooled instances with it
        if (camera == null) warmed.Clear();
        List<GameObject> instances = new List<GameObject>();
        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null || !warmed.Add(prefab)) continue;
            GameObject instance = VFXPool.Get(prefab, stagePosition, Quaternion.identity);
            Simulate(instance);
            instances.Add(instance);
        }
        if (instances.Count == 0) return;

        GetCamera().Render();
        foreach (GameObject instance in instances)
            VFXPool.Release(instance);
    }

    /// <summary>
    /// Advances the effects so that their particles exist when rendered, the delayed ones too
    /// </summary>
    private static void Simulate(GameObject instance)
    {
        foreach (ParticleSystem system in instance.GetComponentsInChildren<ParticleSystem>())
            system.Simulate(system.main.startDelay.constantMax + 0.2f, false, true);
        foreach (VisualEffect effect in instance.GetComponentsInChildren<VisualEffect>())
        {
            effect.Reinit();
            effect.Simulate(1 / 30f, 10);
        }
    }

    /// <summary>
    /// A disabled camera rendering the stage to a small texture, with the pipeline settings of the game camera
    /// </summary>
    private static Camera GetCamera()
    {
        if (camera != null) return camera;
        GameObject cameraObject = new GameObject("VFX Warmup Camera");
        cameraObject.transform.SetPositionAndRotation(stagePosition + new Vector3(0, 3, -6), Quaternion.Euler(25, 0, 0));
        camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.orthographic = true;
        camera.orthographicSize = 4;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 30;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.allowHDR = true;
        camera.allowMSAA = true;
        RenderTextureDescriptor descriptor = new RenderTextureDescriptor(128, 128, RenderTextureFormat.DefaultHDR, 24);
        if (UniversalRenderPipeline.asset != null) descriptor.msaaSamples = UniversalRenderPipeline.asset.msaaSampleCount;
        camera.targetTexture = new RenderTexture(descriptor) { name = "VFX Warmup" };
        UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = false;
        cameraData.renderShadows = false;
        return camera;
    }
}
