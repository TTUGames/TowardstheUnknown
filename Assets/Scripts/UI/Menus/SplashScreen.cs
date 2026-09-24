using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

/// <summary>
/// Fades the studio logo in and out while the main menu loads, then opens it. Any button skips to the fade out.
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup logo;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private int menuBuildIndex = 1;

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
        AsyncOperation loading = SceneManager.LoadSceneAsync(menuBuildIndex);
        loading.allowSceneActivation = false;

        logo.alpha = 0f;
        while (logo.alpha < 1f && !skipped)
        {
            logo.alpha += Time.deltaTime / fadeInDuration;
            yield return null;
        }
        for (float time = 0f; time < holdDuration && !skipped; time += Time.deltaTime)
            yield return null;
        //A skip during the fade in fades out from the current alpha
        while (logo.alpha > 0f)
        {
            logo.alpha -= Time.deltaTime / fadeOutDuration;
            yield return null;
        }

        loading.allowSceneActivation = true;
    }
}
