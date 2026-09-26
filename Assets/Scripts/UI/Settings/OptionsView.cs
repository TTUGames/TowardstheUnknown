using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

/// <summary>
/// Binds the settings view (Assets/UI/Menus/Options.uxml) shared by the main menu and the pause menu
/// </summary>
public class OptionsView
{
    // The settings reset by each page, in the order of the pages and their tabs: gameplay, video, audio
    private static readonly GameSetting[][] pageSettings = {
        new[] { GameSetting.ScreenShake, GameSetting.GameSpeed },
        new[] { GameSetting.Luminosity, GameSetting.Contrast, GameSetting.Fullscreen, GameSetting.VSync },
        new[] { GameSetting.MasterVolume, GameSetting.MusicVolume, GameSetting.SFXVolume },
    };
    private const string SelectedTabClassName = "options-tab--selected";
    private const string SelectedLanguageClassName = "outline-button--selected";
    private const string ValueClassName = "setting__value";

    private readonly VisualElement root;
    private readonly VisualElement languages;
    private readonly List<VisualElement> tabs;
    private readonly List<VisualElement> pages;
    private int page;

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
            {
                Slider slider = Slider(setting);
                slider.fill = true;
                // Its value, after it on its line
                var value = new Label();
                value.AddToClassList(ValueClassName);
                slider.parent.Add(value);
                slider.RegisterValueChangedCallback(evt => {
                    GameSettings.Set(boundSetting, evt.newValue);
                    value.text = FormatValue(boundSetting, evt.newValue);
                });
            }
        }
        root.Q<Button>("Reset").clicked += () => ResetToDefault(pageSettings[page]);
        root.Q<Button>("Back").clicked += back;

        tabs = root.Query(className: "options-tab").ToList();
        pages = root.Query(className: "options__page").ToList();
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            ((Button)tabs[i]).clicked += () => ShowPage(index);
        }

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
        ShowPage(0);
        HighlightLanguage();
        foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
            if (GameSettings.IsSwitch(setting)) ShowSwitch(setting);
            else ShowSlider(setting, GameSettings.Get(setting));
    }

    private void ShowPage(int index)
    {
        page = index;
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].EnableInClassList("hidden", i != index);
            tabs[i].EnableInClassList(SelectedTabClassName, i == index);
        }
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

    private void ShowSlider(GameSetting setting, float value)
    {
        Slider slider = Slider(setting);
        slider.SetValueWithoutNotify(value);
        slider.parent.Q<Label>(className: ValueClassName).text = FormatValue(setting, value);
    }

    /// <summary>
    /// A slider's value as shown after it: the volumes and the shake in percent, the speed as a factor, the image offsets signed
    /// </summary>
    private static string FormatValue(GameSetting setting, float value) => setting switch {
        GameSetting.GameSpeed => "x" + value.ToString("0.0", CultureInfo.InvariantCulture),
        GameSetting.Luminosity => value.ToString("+0.0;-0.0;0.0", CultureInfo.InvariantCulture),
        GameSetting.Contrast => value.ToString("+0;-0;0", CultureInfo.InvariantCulture),
        _ => value.ToString("0", CultureInfo.InvariantCulture) + "%",
    };

    private void ResetToDefault(GameSetting[] settings)
    {
        foreach (GameSetting setting in settings)
        {
            float value = GameSettings.Default(setting);
            GameSettings.Set(setting, value);
            if (GameSettings.IsSwitch(setting)) ShowSwitch(setting);
            else ShowSlider(setting, value);
        }
    }
}
