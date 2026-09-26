using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Covers the screen with a <see cref="SlantedWipe"/>, loads a scene in the background, then reveals the new scene.
/// Created by <see cref="GameFlow"/> and kept across the load; the wipe blocks the pointer while it is shown
/// </summary>
public class SceneTransition : MonoBehaviour
{
    // Above every other document of the panel
    private const float SortingOrder = 1000;

    private SlantedWipe wipe;

    /// <summary>
    /// Loads the build scene <paramref name="sceneIndex"/> behind a wipe, then calls <paramref name="onDone"/>
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

        // The wipe of the room transitions (Common.uss)
        transition.wipe = new SlantedWipe();
        transition.wipe.AddToClassList("slanted-wipe");
        transition.wipe.AddToClassList("stretch");
        document.rootVisualElement.Add(transition.wipe);

        transition.StartCoroutine(transition.Run(sceneIndex, onDone));
    }

    private IEnumerator Run(int sceneIndex, Action onDone)
    {
        yield return wipe.Cover(unscaledTime: true);
        yield return SceneManager.LoadSceneAsync(sceneIndex);
        // Lets the new scene run its Start (the map loads its first room)
        yield return null;
        yield return wipe.Reveal(unscaledTime: true);
        onDone();
        Destroy(gameObject);
    }
}
