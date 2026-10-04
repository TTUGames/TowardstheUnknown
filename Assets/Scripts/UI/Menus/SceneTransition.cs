using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Covers the screen with a <see cref="SlantedWipe"/>, loads a scene in the background or changes what is shown
/// (the <see cref="Edition"/>), then reveals it; a scene load is a cut when the edition's profile has no sceneWipe. Created by <see cref="GameFlow"/> and the options, kept across a load;
/// the wipe blocks the pointer while it is shown
/// </summary>
public class SceneTransition : MonoBehaviour
{
    // Above every other document of the panel
    private const float SortingOrder = 1000;

    private SlantedWipe wipe;
    private static int playing;
    // How long the announcement after the reveal stays, then fades out (UssTime), set on it by Common.uss
    private static readonly CustomStyleProperty<string> showDurationProperty = new("--show-duration");
    private static readonly CustomStyleProperty<string> hideDurationProperty = new("--hide-duration");
    // A long cover shows it is working: a diamond turning in a corner, from then on (--loading-delay of .loading-diamond)
    private static readonly CustomStyleProperty<string> loadingDelayProperty = new("--loading-delay");
    private const float LoadingTurnSpeed = 270;

    /// <summary>
    /// A transition covers the screen, or is about to
    /// </summary>
    public static bool IsPlaying => playing > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => playing = 0;

    /// <summary>
    /// Loads the build scene <paramref name="sceneIndex"/> behind a wipe, then calls <paramref name="onDone"/>
    /// </summary>
    public static void Play(int sceneIndex, Action onDone)
    {
        SceneTransition transition = Create();
        // The original release cut to the loaded scene
        transition.wipe.Instant = !Edition.Profile.sceneWipe;
        transition.Run(LoadScene(sceneIndex), onDone);
    }

    /// <summary>
    /// Calls <paramref name="whileCovered"/> behind a wipe, then <paramref name="onDone"/> once the screen is revealed, and shows
    /// the text <paramref name="announcement"/> gives then, if any, a moment at the top of the screen
    /// </summary>
    public static void Play(Action whileCovered, Action onDone, Func<string> announcement = null) =>
        Create().Run(Call(whileCovered), onDone, announcement);

    private static SceneTransition Create()
    {
        // Inactive until configured: the UIDocument joins its panel in OnEnable
        GameObject go = new GameObject(nameof(SceneTransition));
        go.SetActive(false);
        DontDestroyOnLoad(go);
        UIDocument document = go.AddComponent<UIDocument>();
        document.panelSettings = GameAssets.Instance.panelSettings;
        document.sortingOrder = SortingOrder;
        SceneTransition transition = go.AddComponent<SceneTransition>();
        go.SetActive(true);

        // The wipe of the room transitions (Common.uss)
        transition.wipe = new SlantedWipe();
        transition.wipe.AddToClassList("slanted-wipe");
        transition.wipe.AddToClassList("stretch");
        document.rootVisualElement.Add(transition.wipe);
        // A cut (EditionProfile.sceneWipe off) is silent
        UISounds sounds = GameAssets.Instance.uiSounds;
        if (sounds != null)
        {
            transition.wipe.Covering += () => sounds.wipeCover.Post(go);
            transition.wipe.Revealing += () => sounds.wipeReveal.Post(go);
        }
        return transition;
    }

    private void Run(IEnumerator whileCovered, Action onDone, Func<string> announcement = null) => StartCoroutine(Transition(whileCovered, onDone, announcement));

    private IEnumerator Transition(IEnumerator whileCovered, Action onDone, Func<string> announcement)
    {
        playing++;
        yield return wipe.Cover(unscaledTime: true);
        // A cut (the Classic) shows nothing while it loads
        VisualElement loading = wipe.Instant ? null : AddLoadingDiamond();
        bool covered = true;
        if (loading != null) StartCoroutine(TurnLoadingDiamond(loading, () => covered));
        yield return whileCovered;
        covered = false;
        loading?.RemoveFromHierarchy();
        yield return wipe.Reveal(unscaledTime: true);
        playing--;
        onDone?.Invoke();
        if (announcement != null) yield return Announce(announcement());
        Destroy(gameObject);
    }

    private VisualElement AddLoadingDiamond()
    {
        var diamond = new VisualElement { pickingMode = PickingMode.Ignore };
        diamond.AddToClassList("loading-diamond");
        GetComponent<UIDocument>().rootVisualElement.Add(diamond);
        return diamond;
    }

    // Shown once the cover lasts, turning in real time while the screen stays covered
    private static IEnumerator TurnLoadingDiamond(VisualElement diamond, Func<bool> covered)
    {
        // Once its style is resolved
        yield return null;
        diamond.customStyle.TryGetSeconds(loadingDelayProperty, out float delay);
        float start = Time.unscaledTime;
        while (covered())
        {
            float time = Time.unscaledTime - start;
            diamond.EnableInClassList("loading-diamond--shown", time >= delay);
            diamond.style.rotate = new Rotate(45 + time * LoadingTurnSpeed);
            yield return null;
        }
    }

    /// <summary>
    /// A label at the top of the 16:9 area (edition-toast of Common.uss, the Classic's plain rectangle) that fades in, stays,
    /// then fades out; it ignores the pointer, and the transition no longer counts as playing meanwhile
    /// </summary>
    private IEnumerator Announce(string text)
    {
        var area = new VisualElement { pickingMode = PickingMode.Ignore };
        area.AddToClassList("stretch");
        var toast = new SlantedLabel { text = text, pickingMode = PickingMode.Ignore, corners = Corners.TopLeft | Corners.BottomRight };
        toast.AddToClassList("edition-toast");
        toast.AddToClassList("panel");
        area.Add(toast);
        GetComponent<UIDocument>().rootVisualElement.Add(area);
        MenuScreen.Setup(area, gameObject, GameAssets.Instance.uiSounds, originalSounds: false);
        // Once its style is resolved, so that its opacity transitions
        yield return null;
        toast.AddToClassList("edition-toast--shown");
        toast.customStyle.TryGetSeconds(showDurationProperty, out float show);
        yield return new WaitForSecondsRealtime(show);
        toast.RemoveFromClassList("edition-toast--shown");
        toast.customStyle.TryGetSeconds(hideDurationProperty, out float hide);
        yield return new WaitForSecondsRealtime(hide);
    }

    private static IEnumerator LoadScene(int sceneIndex)
    {
        yield return SceneManager.LoadSceneAsync(sceneIndex);
        // Lets the new scene run its Start (the map loads its first room)
        yield return null;
    }

    private static IEnumerator Call(Action action)
    {
        action();
        // Lets the change show: the new materials render and the style sheets resolve
        yield return null;
    }
}
