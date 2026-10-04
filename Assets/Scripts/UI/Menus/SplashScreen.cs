using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Fades the studio logo (Assets/UI/Menus/Splash.uxml) in and out while the main menu loads, then opens it.
/// Any button skips to the fade out. Meanwhile the sound engine starts and loads the game's bank in the background: the
/// menu's WwiseGlobal then finds them ready (its bank load only counts a reference) instead of freezing on the 15 MB bank
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField, Tooltip("The bank WwiseGlobal loads, loaded here in the background")] private AK.Wwise.Bank bank = new AK.Wwise.Bank();
    [SerializeField, Tooltip("Seconds the menu waits at most for the bank, after the logo")] private float bankTimeout = 10f;

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
        bool bankLoaded = !LoadBankAsync(() => bankLoaded = true);

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

        for (float time = 0f; !bankLoaded && time < bankTimeout; time += Time.unscaledDeltaTime)
            yield return null;
        loading.allowSceneActivation = true;
    }

    /// <summary>
    /// Starts the sound engine (an AkInitializer, kept across the scenes; WwiseGlobal's own then stands down) and loads the
    /// bank in the background, kept loaded for the game: false if there is nothing to wait for
    /// </summary>
    private bool LoadBankAsync(Action loaded)
    {
        if (!bank.IsValid()) return false;
        if (!AkUnitySoundEngine.IsInitialized()) new GameObject(nameof(AkInitializer)).AddComponent<AkInitializer>();
        if (!AkUnitySoundEngine.IsInitialized()) return false;
        bank.LoadAsync((id, memory, result, cookie) => loaded());
        return true;
    }
}
