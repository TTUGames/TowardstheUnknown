using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Fades the studio logo (Assets/UI/Menus/Splash.uxml) in and out while the main menu loads, then opens it.
/// Any button skips to the fade out.
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;

    private bool skipped;
    private IDisposable anyButtonListener;

    private void OnEnable()
    {
        anyButtonListener = InputSystem.onAnyButtonPress.CallOnce(_ => skipped = true);
    }

    private void OnDisable()
    {
        anyButtonListener?.Dispose();
    }

    private IEnumerator Start()
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(GameFlow.MainMenuScene);
        loading.allowSceneActivation = false;

        VisualElement logo = document.rootVisualElement.Q("Logo");
        float alpha = 0f;
        while (alpha < 1f && !skipped)
        {
            alpha = Mathf.Min(1f, alpha + Time.deltaTime / fadeInDuration);
            logo.style.opacity = alpha;
            yield return null;
        }
        for (float time = 0f; time < holdDuration && !skipped; time += Time.deltaTime)
            yield return null;
        //A skip during the fade in fades out from the current alpha
        while (alpha > 0f)
        {
            alpha = Mathf.Max(0f, alpha - Time.deltaTime / fadeOutDuration);
            logo.style.opacity = alpha;
            yield return null;
        }

        loading.allowSceneActivation = true;
    }
}
