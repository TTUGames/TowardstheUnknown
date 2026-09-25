using System;
using System.Globalization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

/// <summary>
/// Binds the settings view (Assets/UI/Menus/Options.uxml) shared by the main menu and the pause menu
/// </summary>
public class OptionsView
{
    private static readonly GameSetting[] videoSettings = { GameSetting.Luminosity, GameSetting.Contrast, GameSetting.Fullscreen, GameSetting.VSync };
    private static readonly GameSetting[] audioSettings = { GameSetting.MasterVolume, GameSetting.MusicVolume, GameSetting.SFXVolume };
    private static readonly GameSetting[] gameplaySettings = { GameSetting.ScreenShake, GameSetting.GameSpeed };
    private const string SelectedLanguageClassName = "outline-button--selected";

    private readonly VisualElement root;
    private readonly VisualElement languages;

    /// <param name="root">The Options.uxml instance</param>
    /// <param name="back">Called by the back button</param>
    public OptionsView(VisualElement root, Action back)
    {
        this.root = root;
        foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
        {
            GameSetting boundSetting = setting;
            if (GameSettings.IsSwitch(setting))
                Switch(setting).clicked += () => {
                    GameSettings.Set(boundSetting, GameSettings.Get(boundSetting) > 0 ? 0 : 1);
                    ShowSwitch(boundSetting);
                };
            else
                Slider(setting).RegisterValueChangedCallback(evt => GameSettings.Set(boundSetting, evt.newValue));
        }
        root.Q<Button>("ResetVideo").clicked += () => ResetToDefault(videoSettings);
        root.Q<Button>("ResetAudio").clicked += () => ResetToDefault(audioSettings);
        root.Q<Button>("ResetGameplay").clicked += () => ResetToDefault(gameplaySettings);
        root.Q<Button>("Back").clicked += back;

        languages = root.Q("Languages");
        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            var button = new SlantedButton { corners = Corners.TopLeft | Corners.BottomRight, text = LanguageName(locale), userData = locale };
            button.AddToClassList("outline-button");
            button.AddToClassList("panel");
            button.clicked += () => {
                Localization.SelectLanguage(locale);
                HighlightLanguage();
            };
            languages.Add(button);
        }
    }

    public bool IsShown => !root.ClassListContains("hidden");

    public void Show(bool show)
    {
        root.EnableInClassList("hidden", !show);
        if (!show) return;
        HighlightLanguage();
        foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
            if (GameSettings.IsSwitch(setting)) ShowSwitch(setting);
            else Slider(setting).SetValueWithoutNotify(GameSettings.Get(setting));
    }

    private SlantedButton Switch(GameSetting setting) => root.Q<SlantedButton>(setting.ToString());

    private void ShowSwitch(GameSetting setting)
    {
        bool on = GameSettings.Get(setting) > 0;
        SlantedButton button = Switch(setting);
        button.key = on ? "OptionsOn" : "OptionsOff";
        button.EnableInClassList(SelectedLanguageClassName, on);
    }

    /// <summary>
    /// The name of a language in that language, capitalized: Français, English
    /// </summary>
    private static string LanguageName(Locale locale)
    {
        CultureInfo culture = locale.Identifier.CultureInfo;
        string name = culture != null ? culture.NativeName : locale.LocaleName;
        return name.Length > 0 ? char.ToUpper(name[0], culture ?? CultureInfo.InvariantCulture) + name.Substring(1) : name;
    }

    private void HighlightLanguage()
    {
        foreach (VisualElement button in languages.Children())
            button.EnableInClassList(SelectedLanguageClassName, button.userData == LocalizationSettings.SelectedLocale);
    }

    private Slider Slider(GameSetting setting) => root.Q<Slider>(setting.ToString());

    private void ResetToDefault(GameSetting[] settings)
    {
        foreach (GameSetting setting in settings)
        {
            float value = GameSettings.Default(setting);
            GameSettings.Set(setting, value);
            if (GameSettings.IsSwitch(setting)) ShowSwitch(setting);
            else Slider(setting).SetValueWithoutNotify(value);
        }
    }
}
