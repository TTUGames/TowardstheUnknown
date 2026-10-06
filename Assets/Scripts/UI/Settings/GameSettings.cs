using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum GameSetting { MasterVolume, MusicVolume, SFXVolume, Luminosity, Contrast, ScreenShake, Fullscreen, VSync, UIVolume, AmbienceVolume, LowHealthAudio, RenderScale, ReduceImpact, Resolution, Flashes, TooltipDelay, ReducedParticles, ReducedMotion, TextSize, FrameRateCap }

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
    // The resolutions offered, the screen's own and its 16:9 ones from 1280 x 720, smallest first; null until first read
    private static List<Vector2Int> resolutions;
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
        masterVolume = musicVolume = sfxVolume = uiVolume = ambienceVolume = null;
        ScreenShake = 1;
        ReducedImpact = false;
        Flashes = 1;
        TooltipDelay = 1;
        reducedParticles = null;
        reducedMotion = null;
        textSize = null;
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
        if (setting == GameSetting.FrameRateCap) return FrameRateIndex();
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
        or GameSetting.ReduceImpact or GameSetting.Resolution or GameSetting.ReducedParticles or GameSetting.ReducedMotion or GameSetting.TextSize or GameSetting.FrameRateCap;

    /// <summary>
    /// How many values a <see cref="IsSwitch"/> setting steps through
    /// </summary>
    public static int Choices(GameSetting setting) => setting switch {
        GameSetting.Fullscreen => displayModes.Length,
        GameSetting.Resolution => Resolutions.Count,
        GameSetting.TextSize => textSizeKeys.Length,
        GameSetting.FrameRateCap => FrameRates().Count,
        _ => 2,
    };

    /// <summary>
    /// The UI key of a <see cref="IsSwitch"/> setting's value, or null for the resolution, written by <see cref="ChoiceText"/>
    /// </summary>
    public static string ChoiceKey(GameSetting setting, float value) => setting switch {
        GameSetting.Fullscreen => displayModeKeys[Mathf.Clamp((int)value, 0, displayModeKeys.Length - 1)],
        GameSetting.Resolution or GameSetting.FrameRateCap => null,
        GameSetting.TextSize => textSizeKeys[Mathf.Clamp((int)value, 0, textSizeKeys.Length - 1)],
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
        if (setting == GameSetting.FrameRateCap)
        {
            List<int> rates = FrameRates();
            int index = Mathf.Clamp((int)value, 0, rates.Count - 1);
            // The screen's rate is saved as 0: it follows the screen
            PlayerPrefs.SetInt(FrameRateKey, index == rates.Count - 1 ? 0 : rates[index]);
            ApplyFrameRate();
            Changed?.Invoke(setting);
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
        int saved = PlayerPrefs.GetInt(FrameRateKey, 0);
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
        GameSetting.Resolution => Resolutions.Count - 1,
        GameSetting.FrameRateCap => FrameRates().Count - 1,
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
