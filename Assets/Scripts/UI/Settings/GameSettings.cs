using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GameSetting { MasterVolume, MusicVolume, SFXVolume, Luminosity, Contrast, ScreenShake, Fullscreen, VSync, UIVolume, AmbienceVolume, LowHealthAudio, RenderScale, ReduceImpact, Resolution, Flashes, TooltipDelay, ReducedParticles, ReducedMotion, TextSize, FrameRateCap, Quality, AirQuality, AmbientOcclusion, ShadowQuality, WaterReflections }

/// <summary>
/// Saves the player settings in the settings file (<see cref="Prefs"/>) and applies them to Wwise, to the color adjustments volume, to the camera shake, to the flashes and to the tooltips' delay
/// </summary>
public static class GameSettings
{
    private static Volume colorVolume;
    // The contrast the places add to the setting's (ContrastOffset, the antechamber's), the settings' volume overriding theirs
    private static float contrastOffset;
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

    /// <summary>
    /// The size of the UI's small and middle texts: 0 as designed, 1 larger, 2 larger still (the text-scale classes of the
    /// screens, <see cref="MenuScreen"/>); read from the saved settings on first use, as <see cref="ReducedParticles"/>
    /// </summary>
    public static int TextSize => textSize ??= Mathf.Clamp((int)Get(GameSetting.TextSize), 0, textSizeKeys.Length - 1);
    private static int? textSize;
    private static readonly string[] textSizeKeys = { "OptionsTextNormal", "OptionsTextLarge", "OptionsTextLarger" };

    // The display mode is applied once per launch: Alt+Enter changes it behind the settings' back, and each scene's Load would undo it
    private static bool fullscreenApplied;
    // The display modes of the Fullscreen setting, by value: 0 and 1 are the values saved before the exclusive mode
    private static readonly FullScreenMode[] displayModes = { FullScreenMode.Windowed, FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen };
    private static readonly string[] displayModeKeys = { "OptionsWindowed", "OptionsBorderless", "OptionsExclusive" };
    // The display mode last seen on the screen, to notice Alt+Enter's; null until the next frame reads it
    private static FullScreenMode? watchedMode;
    // The resolutions offered, the screen's own and its 16:9 ones from 1280 x 720, smallest first, and those a window takes
    // with its frame and the taskbar (WindowShare of the screen); null until first read
    private static List<Vector2Int> resolutions, windowResolutions;
    private const float WindowShare = 0.9f;
    private const string ResolutionKey = "Resolution";
    // The frame rate cap, in frames per second, 0 for the screen's rate; the rates offered without the sync (with it, the
    // screen's rate divided), none under MinFrameRate nor over the screen's
    private const string FrameRateKey = "FrameRateCap";
    private const int MinFrameRate = 30;
    private static readonly int[] frameRates = { 30, 60, 75, 90, 120, 144, 165, 240 };

    // Play mode starts without a domain reload: back to the defaults until Load applies the saved settings
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        colorVolume = null;
        contrastOffset = 0;
        masterVolume = musicVolume = sfxVolume = uiVolume = ambienceVolume = null;
        ScreenShake = 1;
        ReducedImpact = false;
        Flashes = 1;
        TooltipDelay = 1;
        reducedParticles = null;
        reducedMotion = null;
        textSize = null;
        fullscreenApplied = false;
        watchedMode = null;
        Changed = null;
        resolutions = windowResolutions = null;
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
                Prefs.SetFloat(Key(setting), Get(setting));
                continue;
            }
            Apply(setting, Get(setting));
        }
        fullscreenApplied = true;
    }

    /// <summary>
    /// Adds a contrast to the setting's while a place shows (<see cref="ContrastOffset"/>), negative to take it away
    /// </summary>
    public static void AddContrastOffset(float contrast)
    {
        contrastOffset += contrast;
        Apply(GameSetting.Contrast, Get(GameSetting.Contrast));
    }

    /// <summary>
    /// The saved value; the display mode as it is in a build, which Alt+Enter changes without the settings
    /// </summary>
    public static float Get(GameSetting setting)
    {
        if (setting == GameSetting.Resolution) return ResolutionIndex(Offered());
        if (setting == GameSetting.FrameRateCap) return FrameRateIndex();
        // The preset the details match, or Custom
        if (setting == GameSetting.Quality) return GraphicsQuality.Preset();
        if (setting == GameSetting.Fullscreen && !Application.isEditor && fullscreenApplied)
            return Screen.fullScreenMode switch {
                FullScreenMode.Windowed => 0,
                FullScreenMode.ExclusiveFullScreen => 2,
                _ => 1,
            };
        return Prefs.GetFloat(Key(setting), Default(setting));
    }

    /// <summary>
    /// The settings chosen among a few values by a button that steps through them, rather than a slider: on or off, the
    /// display mode, the resolution
    /// </summary>
    public static bool IsSwitch(GameSetting setting) => setting is GameSetting.Fullscreen or GameSetting.VSync or GameSetting.LowHealthAudio
        or GameSetting.ReduceImpact or GameSetting.Resolution or GameSetting.ReducedParticles or GameSetting.ReducedMotion or GameSetting.TextSize or GameSetting.FrameRateCap
        or GameSetting.Quality or GameSetting.AirQuality or GameSetting.AmbientOcclusion or GameSetting.ShadowQuality or GameSetting.WaterReflections;

    /// <summary>
    /// How many values a <see cref="IsSwitch"/> setting steps through
    /// </summary>
    public static int Choices(GameSetting setting) => setting switch {
        GameSetting.Fullscreen => displayModes.Length,
        GameSetting.Resolution => Offered().Count,
        GameSetting.TextSize => textSizeKeys.Length,
        GameSetting.FrameRateCap => FrameRates().Count,
        // The presets the button steps through: Custom is only shown
        GameSetting.Quality or GameSetting.AirQuality => 3,
        _ => 2,
    };

    /// <summary>
    /// The value a <see cref="IsSwitch"/> setting's button steps to: the next one, back to the first after the last; the
    /// quality's presets loop from High to Low, and Custom goes to Low
    /// </summary>
    public static float Next(GameSetting setting)
    {
        float current = Get(setting);
        if (setting == GameSetting.Quality) return current >= GraphicsQuality.High ? GraphicsQuality.Low : current + 1;
        return (current + 1) % Choices(setting);
    }

    /// <summary>
    /// The UI key of a <see cref="IsSwitch"/> setting's value, or null for the resolution, written by <see cref="ChoiceText"/>
    /// </summary>
    public static string ChoiceKey(GameSetting setting, float value) => setting switch {
        GameSetting.Fullscreen => displayModeKeys[Mathf.Clamp((int)value, 0, displayModeKeys.Length - 1)],
        GameSetting.Resolution or GameSetting.FrameRateCap => null,
        GameSetting.TextSize => textSizeKeys[Mathf.Clamp((int)value, 0, textSizeKeys.Length - 1)],
        GameSetting.Quality => GraphicsQuality.PresetKeys[Mathf.Clamp((int)value, 0, GraphicsQuality.PresetKeys.Length - 1)],
        GameSetting.AirQuality => value < 1 ? "OptionsOff" : value < 2 ? "OptionsQualityLow" : "OptionsQualityHigh",
        GameSetting.ShadowQuality => value > 0 ? "OptionsQualityHigh" : "OptionsQualityLow",
        _ => value > 0 ? "OptionsOn" : "OptionsOff",
    };

    /// <summary>
    /// A resolution as shown: 1920 × 1080; a frame rate cap: "60 i/s", the screen's "Écran (120 i/s)"
    /// </summary>
    public static string ChoiceText(GameSetting setting, float value)
    {
        if (setting == GameSetting.FrameRateCap)
        {
            List<int> rates = FrameRates();
            int index = Mathf.Clamp((int)value, 0, rates.Count - 1);
            return string.Format(Localization.UI(index == rates.Count - 1 ? "OptionsFrameRateScreen" : "OptionsFrameRateValue"), rates[index]);
        }
        List<Vector2Int> offered = Offered();
        Vector2Int size = offered[Mathf.Clamp((int)value, 0, offered.Count - 1)];
        return $"{size.x} \u00D7 {size.y}";
    }

    public static void Set(GameSetting setting, float value)
    {
        if (setting == GameSetting.Resolution)
        {
            List<Vector2Int> offered = Offered();
            Vector2Int size = offered[Mathf.Clamp((int)value, 0, offered.Count - 1)];
            Prefs.SetString(ResolutionKey, $"{size.x}x{size.y}");
            ApplyDisplay(Get(GameSetting.Fullscreen));
            return;
        }
        if (setting == GameSetting.FrameRateCap)
        {
            List<int> rates = FrameRates();
            int index = Mathf.Clamp((int)value, 0, rates.Count - 1);
            // The screen's rate is saved as 0: it follows the screen
            Prefs.SetInt(FrameRateKey, index == rates.Count - 1 ? 0 : rates[index]);
            ApplyFrameRate();
            Changed?.Invoke(setting);
            return;
        }
        if (setting == GameSetting.Quality)
        {
            // Sets each detail: nothing for Custom, which only reads as the details' own mix
            if (value < GraphicsQuality.Custom) GraphicsQuality.SetPreset(Mathf.RoundToInt(value));
            Changed?.Invoke(setting);
            return;
        }
        Prefs.SetFloat(Key(setting), value);
        Apply(setting, value);
        Changed?.Invoke(setting);
    }

    /// <summary>
    /// The screen's own resolution and its 16:9 and 16:10 ones from 1280 x 720, smallest first (the game's area fills a screen between the two, Letterbox)
    /// </summary>
    private static List<Vector2Int> Resolutions
    {
        get
        {
            if (resolutions != null) return resolutions;
            Vector2Int native = NativeResolution;
            resolutions = Screen.resolutions
                .Select(resolution => new Vector2Int(resolution.width, resolution.height))
                .Where(size => size.x >= 1280 && size.y >= 720 && size.x <= native.x && size.y <= native.y && (size.x * 9 == size.y * 16 || size.x * 10 == size.y * 16))
                .Append(native)
                .Distinct()
                .OrderBy(size => size.x * size.y)
                .ToList();
            return resolutions;
        }
    }

    private static Vector2Int NativeResolution
    {
        get
        {
            Resolution screen = Screen.currentResolution;
            return new Vector2Int(Display.main.systemWidth > 0 ? Display.main.systemWidth : screen.width, Display.main.systemHeight > 0 ? Display.main.systemHeight : screen.height);
        }
    }

    /// <summary>
    /// The resolutions a window takes on the screen, with its frame and the taskbar: at the screen's own, Windows would crop
    /// the window to the screen; the smallest if none fits
    /// </summary>
    private static List<Vector2Int> WindowResolutions
    {
        get
        {
            if (windowResolutions != null) return windowResolutions;
            Vector2Int native = NativeResolution;
            windowResolutions = Resolutions.Where(size => size.x <= native.x * WindowShare && size.y <= native.y * WindowShare).ToList();
            if (windowResolutions.Count == 0) windowResolutions.Add(Resolutions[0]);
            return windowResolutions;
        }
    }

    // The resolutions offered in a display mode: a window's in a window (the editor's game view takes them all)
    private static List<Vector2Int> Offered(FullScreenMode mode) =>
        mode == FullScreenMode.Windowed && !Application.isEditor ? WindowResolutions : Resolutions;

    private static List<Vector2Int> Offered() => Offered(displayModes[Mathf.Clamp((int)Get(GameSetting.Fullscreen), 0, displayModes.Length - 1)]);

    // The saved resolution's place in the list; the largest if none is saved or it isn't offered (a window's are smaller than the screen)
    private static int ResolutionIndex(List<Vector2Int> offered)
    {
        string saved = Prefs.GetString(ResolutionKey, "");
        int index = offered.FindIndex(size => $"{size.x}x{size.y}" == saved);
        return index >= 0 ? index : offered.Count - 1;
    }

    /// <summary>
    /// Called each frame (<see cref="SettingsLoader"/>): Alt+Enter changes the display mode behind the settings' back, with
    /// no event; the mode it chose is saved at once, so that quitting before the next scene keeps it, and a window takes the
    /// chosen resolution rather than a size of its own. The editor's game view is left as it is
    /// </summary>
    public static void WatchDisplayMode()
    {
        if (Application.isEditor || !fullscreenApplied) return;
        FullScreenMode mode = Screen.fullScreenMode;
        if (watchedMode == mode) return;
        bool seen = watchedMode != null;
        watchedMode = mode;
        if (!seen) return;
        float value = Get(GameSetting.Fullscreen);
        Prefs.SetFloat(Key(GameSetting.Fullscreen), value);
        if (mode == FullScreenMode.Windowed) ApplyDisplay(value);
        Changed?.Invoke(GameSetting.Fullscreen);
    }

    // The screen's refresh rate, rounded; 0 if unknown
    private static int RefreshRate => Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);

    /// <summary>
    /// The frame rates the cap offers, lowest first, the screen's last: with the sync, the screen's rate divided by 1 to 4 (the
    /// sync paces the frames, an even share of the screen's); without it, the usual rates under the screen's. None under 30
    /// </summary>
    private static List<int> FrameRates()
    {
        int refresh = RefreshRate;
        var rates = new List<int>();
        if (refresh <= 0)
        {
            rates.Add(0);
            return rates;
        }
        if (Get(GameSetting.VSync) > 0)
            for (int divider = 4; divider >= 2; divider--)
            {
                int rate = Mathf.RoundToInt(refresh / (float)divider);
                if (rate >= MinFrameRate) rates.Add(rate);
            }
        else
            rates.AddRange(frameRates.Where(rate => rate >= MinFrameRate && rate < refresh));
        rates.Add(refresh);
        return rates;
    }

    // The saved cap's place among the rates offered: the highest one not over it, the screen's if none is saved
    private static int FrameRateIndex()
    {
        List<int> rates = FrameRates();
        int saved = Prefs.GetInt(FrameRateKey, 0);
        if (saved <= 0) return rates.Count - 1;
        int index = rates.FindLastIndex(rate => rate <= saved);
        return index >= 0 ? index : 0;
    }

    /// <summary>
    /// The sync and the cap: with the sync, every 1 to 4 screen refreshes (the cap's share of the screen's rate); without it,
    /// the cap, or the screen's rate (frames beyond it would never show)
    /// </summary>
    private static void ApplyFrameRate()
    {
        List<int> rates = FrameRates();
        int refresh = RefreshRate, cap = rates[FrameRateIndex()];
        if (Get(GameSetting.VSync) > 0)
        {
            QualitySettings.vSyncCount = refresh > 0 && cap > 0 ? Mathf.Clamp(Mathf.RoundToInt(refresh / (float)cap), 1, 4) : 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = cap > 0 ? cap : -1;
        }
    }

    public static float Default(GameSetting setting) => setting switch {
        GameSetting.MasterVolume or GameSetting.MusicVolume or GameSetting.SFXVolume or GameSetting.UIVolume or GameSetting.AmbienceVolume => 50,
        GameSetting.ScreenShake or GameSetting.Flashes or GameSetting.TooltipDelay => 100,
        GameSetting.RenderScale => 100,
        GameSetting.Fullscreen or GameSetting.VSync or GameSetting.LowHealthAudio => 1,
        // The best: the authored look
        GameSetting.Quality or GameSetting.AirQuality => GraphicsQuality.High,
        GameSetting.AmbientOcclusion or GameSetting.ShadowQuality or GameSetting.WaterReflections => 1,
        GameSetting.Resolution => Offered().Count - 1,
        GameSetting.FrameRateCap => FrameRates().Count - 1,
        _ => 0,
    };

    //These keys are already stored on the players' computers
    public static string Key(GameSetting setting) => setting switch {
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
            case GameSetting.TextSize:
                textSize = Mathf.Clamp((int)value, 0, textSizeKeys.Length - 1);
                break;
            case GameSetting.Fullscreen:
                ApplyDisplay(value);
                break;
            case GameSetting.VSync:
            case GameSetting.FrameRateCap:
                ApplyFrameRate();
                break;
            case GameSetting.Resolution:
            case GameSetting.Quality:
                break;
            case GameSetting.AirQuality:
            case GameSetting.AmbientOcclusion:
            case GameSetting.ShadowQuality:
            case GameSetting.WaterReflections:
                GraphicsQuality.Apply(setting, value);
                break;
            default:
                if (colorVolume == null || !colorVolume.profile.TryGet(out ColorAdjustments colorAdjustments)) return;
                if (setting == GameSetting.Luminosity) colorAdjustments.postExposure.Override(value);
                else colorAdjustments.contrast.Override(value + contrastOffset);
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
        FullScreenMode target = displayModes[Mathf.Clamp((int)mode, 0, displayModes.Length - 1)];
        List<Vector2Int> offered = Offered(target);
        Vector2Int size = offered[ResolutionIndex(offered)];
        Screen.SetResolution(size.x, size.y, target);
        // The switch lands at the end of the frame: the watch reads the new mode from the next one, not taking it for Alt+Enter's
        watchedMode = null;
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
