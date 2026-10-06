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
        new[] { GameSetting.ScreenShake, GameSetting.Flashes, GameSetting.ReduceImpact, GameSetting.TooltipDelay, GameSetting.ReducedParticles, GameSetting.ReducedMotion },
        new[] { GameSetting.Luminosity, GameSetting.Contrast, GameSetting.Fullscreen, GameSetting.Resolution, GameSetting.VSync, GameSetting.RenderScale, GameSetting.TextSize, GameSetting.FrameRateCap },
        new[] { GameSetting.MasterVolume, GameSetting.MusicVolume, GameSetting.SFXVolume, GameSetting.AmbienceVolume, GameSetting.UIVolume, GameSetting.LowHealthAudio },
        // The controls, which ControlsPage resets
        Array.Empty<GameSetting>(),
    };
    private const int ControlsPageIndex = 3;
    private const string SelectedTabClassName = "options-tab--selected";
    private const string SelectedLanguageClassName = "outline-button--selected";
    private const string ValueClassName = "setting__value";
    private const string RelocalizingClassName = "options--relocalizing";
    // Milliseconds the faded texts hold before fading back in: long enough for the style to take it
    private const long RelocalizingHold = 60;

    private readonly VisualElement root;
    private readonly VisualElement languages;
    private readonly VisualElement editions;
    private readonly List<VisualElement> tabs;
    private readonly List<VisualElement> pages;
    private readonly SecondClick resetConfirm;
    private readonly ControlsPage controls;
    private int page;
    // The calibration's logo and its color at the default settings, from its style
    private readonly VisualElement calibrationMark;
    private UnityEngine.Color calibrationColor = UnityEngine.Color.black;
    private static readonly CustomStyleProperty<UnityEngine.Color> CalibrationColorProperty = new("--calibration-color");
    // Whether the page keys are listened to: while the options are shown
    private bool listening;

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
                    // To the next value, back to the first after the last
                    GameSettings.Set(boundSetting, (GameSettings.Get(boundSetting) + 1) % GameSettings.Choices(boundSetting));
                    ShowSwitch(boundSetting);
                    // The sync changes the frame rates the cap offers
                    if (boundSetting == GameSetting.VSync) ShowSwitch(GameSetting.FrameRateCap);
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
                    if (boundSetting is GameSetting.Luminosity or GameSetting.Contrast) GradeCalibration();
                });
            }
        }
        // Resetting a page asks for a second click: a stray click would lose its settings
        var reset = root.Q<SlantedButton>("Reset");
        resetConfirm = new SecondClick(reset, "MenuConfirm");
        reset.clicked += () => {
            if (Edition.Profile.confirmations && !resetConfirm.Confirm()) return;
            if (page == ControlsPageIndex) controls.ResetToDefault();
            else ResetToDefault(pageSettings[page]);
        };
        root.Q<Button>("Back").clicked += back;
        root.Q<Button>("OpenLogs").clicked += ErrorLog.OpenFolder;

        controls = new ControlsPage(root.Q("Controls"));
        tabs = root.Query(className: "options-tab").ToList();
        pages = root.Query(className: "options__page").ToList();
        // A page that scrolls brings the setting the keyboard or the gamepad focuses into view
        foreach (VisualElement shown in pages)
            if (shown is ScrollView scroll)
                scroll.RegisterCallback<FocusInEvent>(evt => {
                    if (evt.target is VisualElement focused && scroll.contentContainer.Contains(focused)) scroll.ScrollTo(focused);
                });
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            ((Button)tabs[i]).clicked += () => ShowPage(index);
        }

        // The keys turning the pages, as the keyboard names them, on each side of the tabs; clicking one turns the page too
        AddPageKey(root.Q<Label>("PreviousPageKey"), GameInput.Controls.Menus.PreviousPage, "\u2039 {0}", -1);
        AddPageKey(root.Q<Label>("NextPageKey"), GameInput.Controls.Menus.NextPage, "{0} \u203A", 1);

        calibrationMark = root.Q(className: "calibration__mark");
        calibrationMark.RegisterCallback<CustomStyleResolvedEvent>(_ => {
            if (calibrationMark.customStyle.TryGetValue(CalibrationColorProperty, out UnityEngine.Color color)) calibrationColor = color;
            GradeCalibration();
        });

        languages = root.Q("Languages");
        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            SlantedButton button = AddChoice(languages, locale);
            button.text = LanguageName(locale);
            button.clicked += () => {
                Localization.SelectLanguage(locale);
                HighlightLanguage();
                // The values are written as the language writes numbers
                foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
                    if (!GameSettings.IsSwitch(setting)) ShowSlider(setting, GameSettings.Get(setting));
            };
        }

        editions = root.Q("Editions");
        foreach (GameEdition edition in Enum.GetValues(typeof(GameEdition)))
        {
            SlantedButton button = AddChoice(editions, edition);
            button.key = "Edition" + edition;
            // Behind the wipe of the transitions, which blocks the pointer meanwhile
            button.clicked += () => Edition.SwitchTo(edition);
        }
        // Also changed by its key while the options are shown
        void OnEditionChanged(GameEdition edition) => HighlightEdition();
        Edition.Changed += OnEditionChanged;
        root.RegisterCallback<DetachFromPanelEvent>(_ => Edition.Changed -= OnEditionChanged);

        // The texts rewritten in another language fade back in (the Classic's sheet keeps them as they are)
        VisualElement view = root.Q(className: "options");
        void OnLocaleChanged(Locale locale)
        {
            if (view == null || !IsShown) return;
            view.AddToClassList(RelocalizingClassName);
            view.schedule.Execute(() => view.RemoveFromClassList(RelocalizingClassName)).StartingIn(RelocalizingHold);
        }
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        root.RegisterCallback<DetachFromPanelEvent>(_ => LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged);
    }

    public bool IsShown => !root.ClassListContains("hidden");

    /// <summary>
    /// Adds a button of a row of choices (the languages, the editions), holding its value
    /// </summary>
    private static SlantedButton AddChoice(VisualElement row, object value)
    {
        var button = new SlantedButton { corners = Corners.TopLeft | Corners.BottomRight, userData = value };
        button.AddToClassList("outline-button");
        button.AddToClassList("panel");
        row.Add(button);
        return button;
    }

    public void Show(bool show)
    {
        root.EnableInClassList("hidden", !show);
        resetConfirm.Cancel();
        controls.Cancel();
        ListenPageKeys(show);
        // The sliders write as they move: written to disk once the options close, so that a crash doesn't lose them
        if (!show)
        {
            UnityEngine.PlayerPrefs.Save();
            return;
        }
        ShowPage(0);
        ShowLogErrors();
        HighlightLanguage();
        HighlightEdition();
        foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
            if (GameSettings.IsSwitch(setting)) ShowSwitch(setting);
            else ShowSlider(setting, GameSettings.Get(setting));
    }

    // How many errors were logged since the launch, so that a player knows a bug report is worth it
    private void ShowLogErrors()
    {
        int count = ErrorLog.Count;
        root.Q<Label>("LogErrors").text = count > 0 ? string.Format(Localization.UI("OptionsLogErrors"), count) : "";
    }

    private void ShowPage(int index)
    {
        // The reset asked for, and a key awaited, belonged to the page left
        if (index != page) resetConfirm.Cancel();
        controls.Cancel();
        if (index == ControlsPageIndex) controls.Refresh();
        page = index;
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].EnableInClassList("hidden", i != index);
            tabs[i].EnableInClassList(SelectedTabClassName, i == index);
        }
    }

    private void AddPageKey(Label label, UnityEngine.InputSystem.InputAction action, string format, int step)
    {
        label.text = string.Format(format, UnityEngine.InputSystem.InputActionRebindingExtensions.GetBindingDisplayString(action, 0));
        label.RegisterCallback<ClickEvent>(_ => TurnPage(step));
    }

    private void ListenPageKeys(bool listen)
    {
        if (listen == listening) return;
        listening = listen;
        var menus = GameInput.Controls.Menus;
        if (listen)
        {
            menus.PreviousPage.performed += OnPreviousPage;
            menus.NextPage.performed += OnNextPage;
        }
        else
        {
            menus.PreviousPage.performed -= OnPreviousPage;
            menus.NextPage.performed -= OnNextPage;
        }
    }

    private void OnPreviousPage(UnityEngine.InputSystem.InputAction.CallbackContext context) => TurnPage(-1);

    private void OnNextPage(UnityEngine.InputSystem.InputAction.CallbackContext context) => TurnPage(1);

    // To the next or previous page, the last one coming back to the first
    private void TurnPage(int step) => ShowPage((page + step + pages.Count) % pages.Count);

    private SlantedButton Switch(GameSetting setting) => root.Q<SlantedButton>(setting.ToString());

    private void ShowSwitch(GameSetting setting)
    {
        float value = GameSettings.Get(setting);
        SlantedButton button = Switch(setting);
        string key = GameSettings.ChoiceKey(setting, value);
        button.key = key;
        if (key == null) button.text = GameSettings.ChoiceText(setting, value);
        // A switch on reads in the accent; the other choices stay plain
        button.EnableInClassList(SelectedLanguageClassName, key == "OptionsOn");
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
            button.EnableInClassList(SelectedLanguageClassName, (Locale)button.userData == LocalizationSettings.SelectedLocale);
    }

    private void HighlightEdition()
    {
        foreach (VisualElement button in editions.Children())
            button.EnableInClassList(SelectedLanguageClassName, (GameEdition)button.userData == Edition.Current);
    }

    private Slider Slider(GameSetting setting) => root.Q<Slider>(setting.ToString());

    private void ShowSlider(GameSetting setting, float value)
    {
        Slider slider = Slider(setting);
        slider.SetValueWithoutNotify(value);
        slider.parent.Q<Label>(className: ValueClassName).text = FormatValue(setting, value);
        if (setting is GameSetting.Luminosity or GameSetting.Contrast) GradeCalibration();
    }

    // The calibration's logo as the 3D shows its color with the luminosity and contrast settings
    private void GradeCalibration()
    {
        if (calibrationMark == null) return;
        float exposure = GameSettings.Get(GameSetting.Luminosity), contrast = GameSettings.Get(GameSetting.Contrast);
        UnityEngine.Color linear = calibrationColor.linear;
        var graded = new UnityEngine.Color(Grade(linear.r, exposure, contrast), Grade(linear.g, exposure, contrast), Grade(linear.b, exposure, contrast));
        calibrationMark.style.unityBackgroundImageTintColor = graded.gamma;
    }

    // URP's color grading of a linear value: the post exposure (2 to its stops), then the contrast around the middle grey in
    // ACEScc's log space (ALEXA LogC), as its color lookup does
    private static float Grade(float linear, float exposure, float contrast)
    {
        const float cut = 0.011361f, a = 5.555556f, b = 0.047996f, c = 0.244161f, d = 0.386036f, e = 5.301883f, f = 0.092819f;
        const float middleGrey = 0.4135884f;
        float x = linear * UnityEngine.Mathf.Pow(2, exposure);
        float log = x > cut ? c * UnityEngine.Mathf.Log10(a * x + b) + d : e * x + f;
        log = (log - middleGrey) * (1 + contrast / 100) + middleGrey;
        float graded = log > e * cut + f ? (UnityEngine.Mathf.Pow(10, (log - d) / c) - b) / a : (log - f) / e;
        return UnityEngine.Mathf.Max(0, graded);
    }

    /// <summary>
    /// A slider's value as shown after it, in percent as the selected language writes them: the volumes and the shake, the
    /// luminosity as the image's brightness (its exposure, in stops, turned into a factor: 100 % untouched), the contrast signed
    /// </summary>
    private static string FormatValue(GameSetting setting, float value)
    {
        CultureInfo culture = LocalizationSettings.SelectedLocale != null ? LocalizationSettings.SelectedLocale.Identifier.CultureInfo : null;
        culture ??= CultureInfo.InvariantCulture;
        return setting switch {
            GameSetting.Luminosity => Percent(UnityEngine.Mathf.Pow(2, value) * 100, false, culture),
            GameSetting.Contrast => Percent(value, true, culture),
            _ => Percent(value, false, culture),
        };
    }

    // French writes a space before the percent sign (a plain one: the fonts may lack the no-break one)
    private static string Percent(float value, bool signed, CultureInfo culture)
    {
        string number = UnityEngine.Mathf.RoundToInt(value).ToString(signed ? "+0;-0;0" : "0", culture);
        return culture.TwoLetterISOLanguageName == "fr" ? number + " %" : number + "%";
    }

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
