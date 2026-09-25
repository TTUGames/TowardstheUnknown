using System;
using UnityEngine.UIElements;

/// <summary>
/// Binds the settings view (Assets/UI/Menus/Options.uxml) shared by the main menu and the pause menu
/// </summary>
public class OptionsView
{
    private static readonly GameSetting[] videoSettings = { GameSetting.Luminosity, GameSetting.Contrast };
    private static readonly GameSetting[] audioSettings = { GameSetting.MasterVolume, GameSetting.MusicVolume, GameSetting.SFXVolume };

    private readonly VisualElement root;

    /// <param name="root">The Options.uxml instance</param>
    /// <param name="back">Called by the back button</param>
    public OptionsView(VisualElement root, Action back)
    {
        this.root = root;
        foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
        {
            GameSetting boundSetting = setting;
            Slider(setting).RegisterValueChangedCallback(evt => GameSettings.Set(boundSetting, evt.newValue));
        }
        root.Q<Button>("ResetVideo").clicked += () => ResetToDefault(videoSettings);
        root.Q<Button>("ResetAudio").clicked += () => ResetToDefault(audioSettings);
        root.Q<Button>("Back").clicked += back;
    }

    public bool IsShown => !root.ClassListContains("hidden");

    public void Show(bool show)
    {
        root.EnableInClassList("hidden", !show);
        if (!show) return;
        foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
            Slider(setting).SetValueWithoutNotify(GameSettings.Get(setting));
    }

    private Slider Slider(GameSetting setting) => root.Q<Slider>(setting.ToString());

    private void ResetToDefault(GameSetting[] settings)
    {
        foreach (GameSetting setting in settings)
        {
            float value = GameSettings.Default(setting);
            Slider(setting).SetValueWithoutNotify(value);
            GameSettings.Set(setting, value);
        }
    }
}
