using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Behaviours shared by the UI Toolkit menus
/// </summary>
public static class MenuScreen
{
    public const string CapsClassName = "caps";
    public const string ClassicClassName = "classic";
    // Between two ticks of a moving slider, in real time (UssTime), set on it by Common.uss
    private static readonly CustomStyleProperty<string> tickIntervalProperty = new("--tick-interval");
    // Between the appearance of two buttons of a menu list (UssTime), set on it by Common.uss
    private static readonly CustomStyleProperty<string> staggerDelayProperty = new("--stagger-delay");

    /// <summary>
    /// Keeps the screen in the 16:9 area, follows the edition (the classic class), plays the hover and click sounds of the
    /// buttons (the hover's as a slider moves) and uppercases the texts with the caps class (USS has no text-transform)
    /// </summary>
    /// <param name="originalSounds">Whether the original's buttons of this screen played the sounds: otherwise only the
    /// editions with <see cref="EditionProfile.extraUISounds"/> play them</param>
    public static void Setup(VisualElement root, GameObject soundEmitter, UISounds sounds, bool originalSounds = true)
    {
        bool Sounds() => originalSounds || Edition.Profile.extraUISounds;

        Letterbox.Fit(root);
        FollowEdition(root);
        StaggerMenuLists(root);

        // Enter events don't bubble: they are caught on their way down to the hovered button
        root.RegisterCallback<PointerEnterEvent>(evt => {
            if (evt.target is Button button && button.enabledInHierarchy && Sounds())
                sounds.buttonHover.Post(soundEmitter);
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<ClickEvent>(evt => {
            if (evt.target is Button && Sounds())
                sounds.buttonClick.Post(soundEmitter);
        });
        // A slider ticks as it moves, at most every --tick-interval
        float lastTick = float.NegativeInfinity;
        root.RegisterCallback<ChangeEvent<float>>(evt => {
            if (evt.target is not Slider slider || !Edition.Profile.extraUISounds) return;
            slider.customStyle.TryGetSeconds(tickIntervalProperty, out float interval);
            if (Time.unscaledTime - lastTick < interval) return;
            lastTick = Time.unscaledTime;
            sounds.buttonHover.Post(soundEmitter);
        }, TrickleDown.TrickleDown);
        // The focus moved by the keyboard or the gamepad sounds as a hover; a focus given by the code or a click doesn't
        bool navigating = false;
        root.RegisterCallback<NavigationMoveEvent>(_ => {
            navigating = true;
            root.schedule.Execute(() => navigating = false);
        }, TrickleDown.TrickleDown);
        root.RegisterCallback<FocusInEvent>(evt => {
            if (navigating && evt.target is Button && Sounds())
                sounds.buttonHover.Post(soundEmitter);
            navigating = false;
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

    /// <summary>
    /// Focuses the first button shown in <paramref name="container"/> as soon as its screen is displayed (its visibility comes
    /// with its fade, a second at most), so that the keyboard and the gamepad navigate it at once
    /// </summary>
    public static void FocusFirst(VisualElement container)
    {
        float until = Time.unscaledTime + 1;
        bool done = false;
        container.schedule.Execute(() => {
            Button first = FirstShownButton(container);
            if (first == null) return;
            first.Focus();
            done = true;
        }).Every(0).Until(() => done || Time.unscaledTime > until);
    }

    private static Button FirstShownButton(VisualElement container) =>
        container.Query<Button>().Where(button => button.focusable && button.enabledInHierarchy && button.IsShown()).First();

    /// <summary>
    /// Focuses the first button shown in <paramref name="container"/> on the first navigation move while nothing in it has the
    /// focus: a screen where a reflex press of the submit key (Space) must not click a button
    /// </summary>
    public static void FocusFirstOnNavigation(VisualElement container)
    {
        container.RegisterCallback<NavigationMoveEvent>(evt => {
            Focusable focused = container.focusController?.focusedElement;
            if (focused is VisualElement element && container.Contains(element)) return;
            Button first = FirstShownButton(container);
            if (first == null) return;
            first.Focus();
            evt.StopPropagation();
        }, TrickleDown.TrickleDown);
    }

    /// <summary>
    /// In a development build (and the editor), writes the run's map seed in the label, to replay it (RandomMapGeneration's
    /// seed field); hides it otherwise or on a test map
    /// </summary>
    public static void ShowDevSeed(Label label)
    {
        int seed = Debug.isDebugBuild && GameScene.Map != null ? GameScene.Map.Seed : 0;
        label.text = "Seed " + seed;
        label.style.display = seed != 0 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// The <c>classic</c> class on the root while the Classic edition is shown: the Classic sheets (Classic.uss and its area
    /// sheets, put on the root by <see cref="ClassicStyles"/>) restyle the screen under it
    /// </summary>
    private static void FollowEdition(VisualElement root)
    {
        void Show(GameEdition edition) => root.EnableInClassList(ClassicClassName, edition == GameEdition.Classic);

        Show(Edition.Current);
        Edition.Changed += Show;
        if (root.panel != null) ClassicStyles.Attach(root);
        root.RegisterCallback<DetachFromPanelEvent>(_ => {
            Edition.Changed -= Show;
            ClassicStyles.Detach(root);
        });
        root.RegisterCallback<AttachToPanelEvent>(_ => {
            Edition.Changed -= Show;
            Edition.Changed += Show;
            Show(Edition.Current);
            ClassicStyles.Attach(root);
        });
    }

    /// <summary>
    /// The buttons of the menu lists slide in one after the other (transitions of Common.uss: translate, opacity, color),
    /// --stagger-delay apart: set at once if the list's style is resolved, and again each time it is
    /// </summary>
    private static void StaggerMenuLists(VisualElement root)
    {
        root.Query(className: "menu-list").ForEach(list => {
            void Stagger()
            {
                list.customStyle.TryGetSeconds(staggerDelayProperty, out float stagger);
                int index = 0;
                foreach (VisualElement child in list.Children())
                {
                    if (child is not MenuButton) continue;
                    float delay = stagger * index++;
                    child.style.transitionDelay = new List<TimeValue> { new(delay), new(delay), new(0) };
                }
            }
            Stagger();
            list.RegisterCallback<CustomStyleResolvedEvent>(_ => Stagger());
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
/// A button asking for a second click within its --confirm-duration (UssTime): in between, it reads its confirm key and
/// has the confirm class
/// </summary>
public class SecondClick
{
    private static readonly CustomStyleProperty<string> durationProperty = new("--confirm-duration");

    private readonly Button button;
    private readonly string confirmKey;
    private string key;
    private IVisualElementScheduledItem reset;

    /// <summary>
    /// The first click was made, the second one is awaited
    /// </summary>
    public bool Pending { get; private set; }

    /// <param name="button">A button with a localized text key (<see cref="ILocalizedText"/>)</param>
    public SecondClick(Button button, string confirmKey)
    {
        this.button = button;
        this.confirmKey = confirmKey;
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
        reset = button.schedule.Execute(Cancel).StartingIn(button.customStyle.Milliseconds(durationProperty, 0));
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
