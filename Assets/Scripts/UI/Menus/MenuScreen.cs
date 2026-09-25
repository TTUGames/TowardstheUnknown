using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Behaviours shared by the UI Toolkit menus
/// </summary>
public static class MenuScreen
{
    /// <summary>
    /// Plays the hover and click sounds of the buttons and uppercases the texts with the caps class (USS has no text-transform)
    /// </summary>
    public static void Setup(VisualElement root, GameObject soundEmitter)
    {
        // Enter events don't bubble: they are caught on their way down to the hovered button
        root.RegisterCallback<PointerEnterEvent>(evt => {
            if (evt.target is Button button && button.enabledInHierarchy)
                AkUnitySoundEngine.PostEvent("Button_Hover", soundEmitter);
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<ClickEvent>(evt => {
            if (evt.target is Button)
                AkUnitySoundEngine.PostEvent("Button_Click", soundEmitter);
        });

        // The localized texts can arrive after the setup
        root.Query<TextElement>(className: "caps").ForEach(Uppercase);
        root.RegisterCallback<ChangeEvent<string>>(evt => {
            if (evt.target is TextElement text && text.ClassListContains("caps"))
                Uppercase(text);
        }, TrickleDown.TrickleDown);
    }

    private static void Uppercase(TextElement text)
    {
        string upper = text.text.ToUpperInvariant();
        if (upper != text.text)
            ((INotifyValueChanged<string>)text).SetValueWithoutNotify(upper);
    }
}
