using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GameSetting { MasterVolume, MusicVolume, SFXVolume, Luminosity, Contrast, ScreenShake, Fullscreen, VSync, UIVolume, AmbienceVolume, LowHealthAudio, RenderScale, ReduceImpact, Resolution, Flashes, TooltipDelay, ReducedParticles, ReducedMotion }

/// <summary>
/// Saves the player settings in the PlayerPrefs and applies them to Wwise, to the color adjustments volume, to the camera shake, to the flashes and to the tooltips' delay
/// </summary>
public static class GameSettings
{
    private static Volume colorVolume;
    private static AK.Wwise.RTPC masterVolume, musicVolume, sfxVolume, uiVolume, ambienceVolume;

    /// <summary>
    /// The strength of the camera shakes, from 0 to 1
    /// </summary>
    public static float ScreenShake { get; private set; } = 1;

    /// <summary>
    /// Raised when a setting is changed (the options), with the setting
    /// </summary>
    public static event System.Action<GameSetting> Changed;

    /// <summary>
    /// The hits neither freeze nor slow the time, and the camera doesn't zoom in on them (an accessibility setting)
    /// </summary>
    public static bool ReducedImpact { get; private set; }

    /// <summary>
    /// The strength of the flashes, from 0 to 1: the hits' white flash, the enemies' flare when hit and their flicker at low health
    /// </summary>
    public static float Flashes { get; private set; } = 1;

    /// <summary>
    /// The factor of the tooltips' delay, from 0 (at once) to 2 (twice as long as the edition's): 1 keeps the edition's delays
    /// </summary>
    public static float TooltipDelay { get; private set; } = 1;

    /// <summary>
    /// Fewer big snowflakes and no wind trails in front of the board (<see cref="ParticleReduction"/>), an accessibility setting.
    /// Read from the saved settings on first use: what reads it as a scene starts may come before <see cref="Load"/>
    /// </summary>
    public static bool ReducedParticles => reducedParticles ??= Get(GameSetting.ReducedParticles) > 0;
    private static bool? reducedParticles;

    /// <summary>
    /// The HUD's loops stop beating and the panels stop sliding (the reduced-motion class of the screens, <see cref="MenuScreen"/>),
    /// an accessibility setting; read from the saved settings on first use, as <see cref="ReducedParticles"/>
    /// </summary>
    public static bool ReducedMotion => reducedMotion ??= Get(GameSetting.ReducedMotion) > 0;
    private static bool? reducedMotion;

    // The display mode is applied once per launch: Alt+Enter changes it behind the settings' back, and each scene's Load would undo it
    private static bool fullscreenApplied;
    // The display modes of the Fullscreen setting, by value: 0 and 1 are the values saved before the exclusive mode
    private static readonly FullScreenMode[] displayModes = { FullScreenMode.Windowed, FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen };
    private static readonly string[] displayModeKeys = { "OptionsWindowed", "OptionsBorderless", "OptionsExclusive" };
    // The resolutions offered, the screen's own and its 16:9 ones from 1280 x 720, smallest first; null until first read
    private static List<Vector2Int> resolutions;
    private const string ResolutionKey = "Resolution";

    // Play mode starts without a domain reload: back to the defaults until Load applies the saved settings
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        colorVolume = null;
        masterVolume = musicVolume = sfxVolume = uiVolume = ambienceVolume = null;
        ScreenShake = 1;
        ReducedImpact = false;
        Flashes = 1;
        TooltipDelay = 1;
        reducedParticles = null;
        reducedMotion = null;
        fullscreenApplied = false;
        Changed = null;
        resolutions = null;
    }

    /// <summary>
    /// Sets the volume the luminosity and contrast settings are applied to and the game parameters of the volume
    /// settings, then applies every saved setting
    /// </summary>
    public static void Load(Volume volume, AK.Wwise.RTPC master, AK.Wwise.RTPC music, AK.Wwise.RTPC sfx, AK.Wwise.RTPC ui,
        AK.Wwise.RTPC ambience)
    {
        colorVolume = volume;
        masterVolume = master;
        musicVolume = music;
        sfxVolume = sfx;
        uiVolume = ui;
        ambienceVolume = ambience;
        foreach (GameSetting setting in System.Enum.GetValues(typeof(GameSetting)))
        {
            // Set with the display mode
            if (setting == GameSetting.Resolution) continue;
            if (setting == GameSetting.Fullscreen && fullscreenApplied)
            {
                // Keeps the mode Alt+Enter may have chosen, saved for the next launch
                PlayerPrefs.SetFloat(Key(setting), Get(setting));
                continue;
            }
            Apply(setting, Get(setting));
        }
        fullscreenApplied = true;
    }

    /// <summary>
    /// The saved value; the display mode as it is in a build, which Alt+Enter changes without the settings
    /// </summary>
    public static float Get(GameSetting setting)
    {
        if (setting == GameSetting.Resolution) return ResolutionIndex();
        if (setting == GameSetting.Fullscreen && !Application.isEditor && fullscreenApplied)
            return Screen.fullScreenMode switch {
                FullScreenMode.Windowed => 0,
                FullScreenMode.ExclusiveFullScreen => 2,
                _ => 1,
            };
        return PlayerPrefs.GetFloat(Key(setting), Default(setting));
    }

    /// <summary>
    /// The settings chosen among a few values by a button that steps through them, rather than a slider: on or off, the
    /// display mode, the resolution
    /// </summary>
    public static bool IsSwitch(GameSetting setting) => setting is GameSetting.Fullscreen or GameSetting.VSync or GameSetting.LowHealthAudio
        or GameSetting.ReduceImpact or GameSetting.Resolution or GameSetting.ReducedParticles or GameSetting.ReducedMotion;

    /// <summary>
    /// How many values a <see cref="IsSwitch"/> setting steps through
    /// </summary>
    public static int Choices(GameSetting setting) => setting switch {
        GameSetting.Fullscreen => displayModes.Length,
        GameSetting.Resolution => Resolutions.Count,
        _ => 2,
    };

    /// <summary>
    /// The UI key of a <see cref="IsSwitch"/> setting's value, or null for the resolution, written by <see cref="ChoiceText"/>
    /// </summary>
    public static string ChoiceKey(GameSetting setting, float value) => setting switch {
        GameSetting.Fullscreen => displayModeKeys[Mathf.Clamp((int)value, 0, displayModeKeys.Length - 1)],
        GameSetting.Resolution => null,
        _ => value > 0 ? "OptionsOn" : "OptionsOff",
    };

    /// <summary>
    /// A resolution as shown: 1920 × 1080
    /// </summary>
    public static string ChoiceText(GameSetting setting, float value)
    {
        Vector2Int size = Resolutions[Mathf.Clamp((int)value, 0, Resolutions.Count - 1)];
        return $"{size.x} \u00D7 {size.y}";
    }

    public static void Set(GameSetting setting, float value)
    {
        if (setting == GameSetting.Resolution)
        {
            Vector2Int size = Resolutions[Mathf.Clamp((int)value, 0, Resolutions.Count - 1)];
            PlayerPrefs.SetString(ResolutionKey, $"{size.x}x{size.y}");
            ApplyDisplay(Get(GameSetting.Fullscreen));
            return;
        }
        PlayerPrefs.SetFloat(Key(setting), value);
        Apply(setting, value);
        Changed?.Invoke(setting);
    }

    /// <summary>
    /// The screen's own resolution and its 16:9 ones from 1280 x 720, smallest first
    /// </summary>
    private static List<Vector2Int> Resolutions
    {
        get
        {
            if (resolutions != null) return resolutions;
            Resolution screen = Screen.currentResolution;
            var native = new Vector2Int(Display.main.systemWidth > 0 ? Display.main.systemWidth : screen.width, Display.main.systemHeight > 0 ? Display.main.systemHeight : screen.height);
            resolutions = Screen.resolutions
                .Select(resolution => new Vector2Int(resolution.width, resolution.height))
                .Where(size => size.x >= 1280 && size.y >= 720 && size.x <= native.x && size.y <= native.y && size.x * 9 == size.y * 16)
                .Append(native)
                .Distinct()
                .OrderBy(size => size.x * size.y)
                .ToList();
            return resolutions;
        }
    }

    // The saved resolution's place in the list; the screen's own if none is saved or it isn't offered on this screen
    private static int ResolutionIndex()
    {
        string saved = PlayerPrefs.GetString(ResolutionKey, "");
        int index = Resolutions.FindIndex(size => $"{size.x}x{size.y}" == saved);
        return index >= 0 ? index : Resolutions.Count - 1;
    }

    public static float Default(GameSetting setting) => setting switch {
        GameSetting.MasterVolume or GameSetting.MusicVolume or GameSetting.SFXVolume or GameSetting.UIVolume or GameSetting.AmbienceVolume => 50,
        GameSetting.ScreenShake or GameSetting.Flashes or GameSetting.TooltipDelay => 100,
        GameSetting.RenderScale => 100,
        GameSetting.Fullscreen or GameSetting.VSync or GameSetting.LowHealthAudio => 1,
        GameSetting.Resolution => Resolutions.Count - 1,
        _ => 0,
    };

    //These keys are already stored on the players' computers
    private static string Key(GameSetting setting) => setting switch {
        GameSetting.MasterVolume => "MasterVolumeValue",
        GameSetting.MusicVolume => "MusicVolumeValue",
        GameSetting.SFXVolume => "SFXVolumeValue",
        GameSetting.Luminosity => "luminosityValue",
        GameSetting.Contrast => "contrastValue",
        _ => setting.ToString(),
    };

    private static void Apply(GameSetting setting, float value)
    {
        switch (setting)
        {
            case GameSetting.MasterVolume:
                masterVolume?.SetGlobalValue(value);
                break;
            case GameSetting.MusicVolume:
                musicVolume?.SetGlobalValue(value);
                break;
            case GameSetting.SFXVolume:
                sfxVolume?.SetGlobalValue(value);
                break;
            case GameSetting.UIVolume:
                uiVolume?.SetGlobalValue(value);
                break;
            case GameSetting.AmbienceVolume:
                ambienceVolume?.SetGlobalValue(value);
                break;
            case GameSetting.ScreenShake:
                ScreenShake = Mathf.Clamp01(value / 100);
                break;
            case GameSetting.RenderScale:
                ApplyRenderScale(value);
                break;
            case GameSetting.ReduceImpact:
                ReducedImpact = value > 0;
                break;
            case GameSetting.Flashes:
                Flashes = Mathf.Clamp01(value / 100);
                break;
            case GameSetting.TooltipDelay:
                TooltipDelay = Mathf.Clamp(value / 100, 0, 2);
                break;
            case GameSetting.ReducedParticles:
                reducedParticles = value > 0;
                break;
            case GameSetting.ReducedMotion:
                reducedMotion = value > 0;
                break;
            case GameSetting.Fullscreen:
                ApplyDisplay(value);
                break;
            case GameSetting.VSync:
                QualitySettings.vSyncCount = value > 0 ? 1 : 0;
                //Without the sync, frames beyond the screen's rate would never show: the GPU draws no more than it can
                int refreshRate = Mathf.CeilToInt((float)Screen.currentResolution.refreshRateRatio.value);
                Application.targetFrameRate = value > 0 || refreshRate <= 0 ? -1 : refreshRate;
                break;
            case GameSetting.Resolution:
                break;
            default:
                if (colorVolume == null || !colorVolume.profile.TryGet(out ColorAdjustments colorAdjustments)) return;
                if (setting == GameSetting.Luminosity) colorAdjustments.postExposure.Override(value);
                else colorAdjustments.contrast.Override(value);
                break;
        }
    }

    /// <summary>
    /// The 3D drawn at a share of the screen's resolution, then upscaled by STP (the UI stays sharp), on the pipelines of
    /// both editions; at 100 % no upscaling. In the editor, the assets get their values back when Play mode ends
    /// </summary>
    private static void ApplyRenderScale(float percent)
    {
        float scale = Mathf.Clamp(percent, 50, 100) / 100f;
        foreach (GameEdition edition in System.Enum.GetValues(typeof(GameEdition)))
        {
            if (GameAssets.Instance.EditionProfile(edition).renderPipeline is not UniversalRenderPipelineAsset pipeline) continue;
#if UNITY_EDITOR
            RenderScaleRestore.Remember(pipeline);
#endif
            pipeline.renderScale = scale;
#pragma warning disable CS0618 // the upscaler framework's names are not enabled in this project
            pipeline.upscalingFilter = scale < 1 ? UpscalingFilterSelection.STP : UpscalingFilterSelection.Auto;
#pragma warning restore CS0618
        }
    }

    /// <summary>
    /// The display mode with the chosen resolution: the window's size, or the screen's mode in exclusive full screen; the
    /// borderless full screen draws the game at it, scaled to the screen. The editor's game view stays as it is
    /// </summary>
    private static void ApplyDisplay(float mode)
    {
        if (Application.isEditor) return;
        Vector2Int size = Resolutions[ResolutionIndex()];
        Screen.SetResolution(size.x, size.y, displayModes[Mathf.Clamp((int)mode, 0, displayModes.Length - 1)]);
    }
}

#if UNITY_EDITOR
/// <summary>
/// The pipeline assets are project files: the render scale setting changes them during Play mode only
/// </summary>
internal static class RenderScaleRestore
{
    private static readonly System.Collections.Generic.Dictionary<UniversalRenderPipelineAsset, (float scale, int filter)> saved = new();

    public static void Remember(UniversalRenderPipelineAsset pipeline)
    {
        if (saved.Count == 0) UnityEditor.EditorApplication.playModeStateChanged += Restore;
#pragma warning disable CS0618
        if (!saved.ContainsKey(pipeline)) saved[pipeline] = (pipeline.renderScale, (int)pipeline.upscalingFilter);
#pragma warning restore CS0618
    }

    private static void Restore(UnityEditor.PlayModeStateChange state)
    {
        if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
        foreach (var (pipeline, values) in saved)
        {
            pipeline.renderScale = values.scale;
#pragma warning disable CS0618
            pipeline.upscalingFilter = (UpscalingFilterSelection)values.filter;
#pragma warning restore CS0618
        }
        saved.Clear();
        UnityEditor.EditorApplication.playModeStateChanged -= Restore;
    }
}
#endif
