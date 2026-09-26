using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Behaviours shared by the UI Toolkit menus
/// </summary>
public static class MenuScreen
{
    public const string CapsClassName = "caps";
    // Real seconds between two ticks of a moving slider
    private const float SliderTickInterval = 0.08f;

    /// <summary>
    /// Keeps the screen in the 16:9 area, plays the hover and click sounds of the buttons (the hover's as a slider moves)
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
        // A slider ticks as it moves, at most every SliderTickInterval
        float lastTick = float.NegativeInfinity;
        root.RegisterCallback<ChangeEvent<float>>(evt => {
            if (evt.target is not Slider || Time.unscaledTime - lastTick < SliderTickInterval) return;
            lastTick = Time.unscaledTime;
            sounds.buttonHover.Post(soundEmitter);
        }, TrickleDown.TrickleDown);
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

/// <summary>
/// A button asking for a second click within a delay: in between, it reads its confirm key and has the confirm class
/// </summary>
public class SecondClick
{
    private readonly Button button;
    private readonly string confirmKey;
    private readonly long duration;
    private string key;
    private IVisualElementScheduledItem reset;

    /// <summary>
    /// The first click was made, the second one is awaited
    /// </summary>
    public bool Pending { get; private set; }

    /// <param name="button">A button with a localized text key (<see cref="ILocalizedText"/>)</param>
    /// <param name="duration">In milliseconds</param>
    public SecondClick(Button button, string confirmKey, long duration)
    {
        this.button = button;
        this.confirmKey = confirmKey;
        this.duration = duration;
    }

    /// <summary>
    /// Asks for the second click until the delay ends
    /// </summary>
    public void Ask()
    {
        var text = (ILocalizedText)button;
        key = text.key;
        text.key = confirmKey;
        button.AddToClassList("confirm");
        Pending = true;
        reset = button.schedule.Execute(Cancel).StartingIn(duration);
    }

    /// <summary>
    /// Stops asking, the button reading its key again
    /// </summary>
    public void Cancel()
    {
        reset?.Pause();
        if (!Pending) return;
        Pending = false;
        button.RemoveFromClassList("confirm");
        ((ILocalizedText)button).key = key;
    }

    /// <summary>
    /// A click: true on the second one, asking for it otherwise
    /// </summary>
    public bool Confirm()
    {
        if (!Pending)
        {
            Ask();
            return false;
        }
        Cancel();
        return true;
    }
}
