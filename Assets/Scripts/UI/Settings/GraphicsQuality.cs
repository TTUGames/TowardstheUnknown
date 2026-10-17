using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The graphics quality: four details (the rift's air, the ambient occlusion, the shadows, the water's reflections) that the
/// player sets one by one or all at once by a preset (Low, Medium, High, the authored look); the details a preset sets
/// that don't match any preset read as Custom. They are applied to the pipelines of both editions, as the render scale is
/// </summary>
public static class GraphicsQuality
{
    public const int Low = 0, Medium = 1, High = 2, Custom = 3;

    /// <summary>
    /// The UI keys of the presets and of Custom, by value
    /// </summary>
    public static readonly string[] PresetKeys = { "OptionsQualityLow", "OptionsQualityMedium", "OptionsQualityHigh", "OptionsQualityCustom" };

    /// <summary>
    /// The quality's settings, the preset first
    /// </summary>
    public static readonly GameSetting[] Settings = { GameSetting.Quality, GameSetting.AirQuality, GameSetting.AmbientOcclusion, GameSetting.ShadowQuality, GameSetting.WaterReflections };

    // The details a preset sets, in the order of details: the air (0 off, 1 reduced, 2 as authored), the ambient occlusion,
    // the shadows (0 short and sharp, 1 as authored), the water's reflections
    private static readonly GameSetting[] details = { GameSetting.AirQuality, GameSetting.AmbientOcclusion, GameSetting.ShadowQuality, GameSetting.WaterReflections };
    private static readonly int[][] presets = {
        new[] { 0, 0, 0, 0 },
        new[] { 1, 1, 0, 1 },
        new[] { 2, 1, 1, 1 },
    };

    // The reduced air's resolution, a share of the screen's on each side (the authored one is the feature's own)
    private const float ReducedAirScale = 0.35f;
    // What the low shadows are capped to: the distance in meters, the additional lights' atlas, the cascades
    private const float LowShadowDistance = 20;
    private const int LowShadowAtlas = 1024;

    // The values the pipelines and their features had before the first change: the authored look the best level gives back
    private class Authored
    {
        public float shadowDistance;
        public int additionalAtlas;
        public int cascades;
    }
    private static readonly Dictionary<UniversalRenderPipelineAsset, Authored> pipelines = new();
    private static readonly Dictionary<ScriptableRendererFeature, (bool active, float scale)> features = new();

    // Play mode starts without a domain reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        pipelines.Clear();
        features.Clear();
        WaterReflection.Enabled = true;
    }

    /// <summary>
    /// The preset the saved details make, or <see cref="Custom"/>
    /// </summary>
    public static int Preset()
    {
        for (int preset = 0; preset < presets.Length; preset++)
        {
            bool matches = true;
            for (int i = 0; i < details.Length && matches; i++)
                matches = Mathf.RoundToInt(GameSettings.Get(details[i])) == presets[preset][i];
            if (matches) return preset;
        }
        return Custom;
    }

    /// <summary>
    /// Sets every detail to the preset's
    /// </summary>
    public static void SetPreset(int preset)
    {
        for (int i = 0; i < details.Length; i++)
            GameSettings.Set(details[i], presets[Mathf.Clamp(preset, Low, High)][i]);
    }

    /// <summary>
    /// Applies a detail to the pipelines of both editions (the Classic's has no air nor occlusion: only its shadows follow)
    /// </summary>
    public static void Apply(GameSetting setting, float value)
    {
        if (setting == GameSetting.WaterReflections) WaterReflection.Enabled = value > 0;
        foreach (GameEdition edition in System.Enum.GetValues(typeof(GameEdition)))
        {
            if (GameAssets.Instance.EditionProfile(edition).renderPipeline is not UniversalRenderPipelineAsset pipeline) continue;
            Authored authored = Remember(pipeline);
            switch (setting)
            {
                case GameSetting.AirQuality:
                    foreach (ScriptableRendererFeature feature in Features(pipeline))
                        if (feature is RiftVolumetricsFeature air)
                        {
                            air.SetActive(value > 0);
                            air.Scale = value >= High ? features[air].scale : ReducedAirScale;
                        }
                    break;
                case GameSetting.AmbientOcclusion:
                    foreach (ScriptableRendererFeature feature in Features(pipeline))
                        if (feature is ScreenSpaceAmbientOcclusion) feature.SetActive(value > 0);
                    break;
                case GameSetting.ShadowQuality:
                    bool high = value > 0;
                    pipeline.shadowDistance = high ? authored.shadowDistance : Mathf.Min(authored.shadowDistance, LowShadowDistance);
                    pipeline.additionalLightsShadowmapResolution = high ? authored.additionalAtlas : Mathf.Min(authored.additionalAtlas, LowShadowAtlas);
                    pipeline.shadowCascadeCount = high ? authored.cascades : 1;
                    break;
            }
        }
    }

    // The pipeline's authored values and its features', noted before the first change
    private static Authored Remember(UniversalRenderPipelineAsset pipeline)
    {
#if UNITY_EDITOR
        // The pipeline assets are project files: the changes last during Play mode only
        if (pipelines.Count == 0) UnityEditor.EditorApplication.playModeStateChanged += Restore;
#endif
        if (pipelines.TryGetValue(pipeline, out Authored authored)) return authored;
        authored = new Authored { shadowDistance = pipeline.shadowDistance, additionalAtlas = pipeline.additionalLightsShadowmapResolution, cascades = pipeline.shadowCascadeCount };
        pipelines[pipeline] = authored;
        foreach (ScriptableRendererFeature feature in Features(pipeline))
            features[feature] = (feature.isActive, feature is RiftVolumetricsFeature air ? air.Scale : 0);
        return authored;
    }

    private static IEnumerable<ScriptableRendererFeature> Features(UniversalRenderPipelineAsset pipeline)
    {
        var found = new List<ScriptableRendererFeature>();
        foreach (ScriptableRendererData data in pipeline.rendererDataList)
            if (data != null)
                foreach (ScriptableRendererFeature feature in data.rendererFeatures)
                    if (feature != null) found.Add(feature);
        return found;
    }

#if UNITY_EDITOR
    private static void Restore(UnityEditor.PlayModeStateChange state)
    {
        if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
        foreach (var (pipeline, authored) in pipelines)
        {
            pipeline.shadowDistance = authored.shadowDistance;
            pipeline.additionalLightsShadowmapResolution = authored.additionalAtlas;
            pipeline.shadowCascadeCount = authored.cascades;
        }
        foreach (var (feature, authored) in features)
        {
            feature.SetActive(authored.active);
            if (feature is RiftVolumetricsFeature air) air.Scale = authored.scale;
        }
        pipelines.Clear();
        features.Clear();
        UnityEditor.EditorApplication.playModeStateChanged -= Restore;
    }
#endif
}
