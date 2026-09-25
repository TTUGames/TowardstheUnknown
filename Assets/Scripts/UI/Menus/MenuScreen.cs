using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Behaviours shared by the UI Toolkit menus
/// </summary>
public static class MenuScreen
{
    public const string CapsClassName = "caps";

    /// <summary>
    /// Keeps the screen in the 16:9 area, plays the hover and click sounds of the buttons
    /// and uppercases the texts with the caps class (USS has no text-transform)
    /// </summary>
    public static void Setup(VisualElement root, GameObject soundEmitter, UISounds sounds)
    {
        Letterbox.Fit(root);
        StaggerMenuLists(root);

        // Enter events don't bubble: they are caught on their way down to the hovered button
        root.RegisterCallback<PointerEnterEvent>(evt => {
            if (evt.target is Button button && button.enabledInHierarchy)
                sounds.buttonHover.Post(soundEmitter);
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<ClickEvent>(evt => {
            if (evt.target is Button)
                sounds.buttonClick.Post(soundEmitter);
        });
        // A click focuses the button: the focus is only kept for keyboard and gamepad navigation
        root.RegisterCallback<PointerLeaveEvent>(evt => {
            if (evt.target is Button button && button.focusController?.focusedElement == button)
                button.Blur();
        }, TrickleDown.TrickleDown);

        // The localized texts can arrive after the setup
        root.Query<TextElement>(className: CapsClassName).ForEach(Uppercase);
        root.RegisterCallback<ChangeEvent<string>>(evt => {
            if (evt.target is TextElement text && text.ClassListContains(CapsClassName))
                Uppercase(text);
        }, TrickleDown.TrickleDown);
    }

    // Between the appearance of two buttons of a menu list
    private const float StaggerDelay = 0.05f;

    /// <summary>
    /// The buttons of the menu lists slide in one after the other (transitions of Common.uss: translate, opacity, color)
    /// </summary>
    private static void StaggerMenuLists(VisualElement root)
    {
        root.Query(className: "menu-list").ForEach(list => {
            int index = 0;
            foreach (VisualElement child in list.Children())
            {
                if (child is not MenuButton) continue;
                float delay = StaggerDelay * index++;
                child.style.transitionDelay = new List<TimeValue> { new(delay), new(delay), new(0) };
            }
        });
    }

    private static void Uppercase(TextElement text)
    {
        string upper = text.text.ToUpperInvariant();
        if (upper != text.text)
            ((INotifyValueChanged<string>)text).SetValueWithoutNotify(upper);
    }
}
