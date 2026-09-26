using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Fades the screen to black, loads a scene in the background, then fades the new scene in.
/// Created by <see cref="GameFlow"/> and kept across the load; its black overlay blocks the pointer while it is shown
/// </summary>
public class SceneTransition : MonoBehaviour
{
    // Opacity per second
    private const float Speed = 4;
    // Above every other document of the panel
    private const float SortingOrder = 1000;
    // The frames that follow a load are long: cap their duration so that the fade in stays visible
    private const float MaxFrameDuration = 1f / 30f;

    private VisualElement overlay;

    /// <summary>
    /// Loads the build scene <paramref name="sceneIndex"/> behind a fade, then calls <paramref name="onDone"/>
    /// </summary>
    public static void Play(int sceneIndex, Action onDone)
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

        // The black screen of the room transitions (Common.uss)
        transition.overlay = new VisualElement { pickingMode = PickingMode.Position };
        transition.overlay.AddToClassList("fade");
        transition.overlay.style.opacity = 0;
        document.rootVisualElement.Add(transition.overlay);

        transition.StartCoroutine(transition.Run(sceneIndex, onDone));
    }

    private IEnumerator Run(int sceneIndex, Action onDone)
    {
        yield return Fade(1);
        yield return SceneManager.LoadSceneAsync(sceneIndex);
        // Lets the new scene run its Start (the map loads its first room)
        yield return null;
        yield return Fade(0);
        onDone();
        Destroy(gameObject);
    }

    private IEnumerator Fade(float targetOpacity)
    {
        float opacity = overlay.style.opacity.value;
        while (opacity != targetOpacity)
        {
            opacity = Mathf.MoveTowards(opacity, targetOpacity, Speed * Mathf.Min(Time.unscaledDeltaTime, MaxFrameDuration));
            overlay.style.opacity = opacity;
            yield return null;
        }
    }
}
