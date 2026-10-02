using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum GameEdition { Anniversary, Classic }

/// <summary>
/// The edition the game is shown in: the Anniversary, or the Classic, the look and feel of the original release (its shaders,
/// level art, feedbacks and UX). The gameplay is the same in both. Saved in the PlayerPrefs.
/// The visuals follow <see cref="Changed"/> by themselves: <see cref="EditionOnly"/>, <see cref="EditionMaterials"/>,
/// the <see cref="Profile"/> and the <c>classic</c> class of the UI documents; nothing else tests the edition
/// </summary>
public static class Edition
{
    private const string Key = "Edition";

    private static GameEdition? current;
    private static bool switching;

    /// <summary>
    /// Fired once the edition is set, its render pipeline and materials applied
    /// </summary>
    public static event System.Action<GameEdition> Changed;

    public static GameEdition Current => current ??= (GameEdition)PlayerPrefs.GetInt(Key, (int)GameEdition.Anniversary);

    public static bool IsClassic => Current == GameEdition.Classic;

    /// <summary>
    /// The settings of the current edition, for the systems that can't simply be turned off
    /// </summary>
    public static EditionProfile Profile => GameAssets.Instance.EditionProfile(Current);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        current = null;
        switching = false;
        Changed = null;
    }

    // Before the first scene, so that a game started in the Classic never shows an Anniversary frame
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyOnStart()
    {
        ApplyPipeline();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => EditionMaterials.Apply(scene);

    /// <summary>
    /// Changes the edition behind the wipe of the scene transitions, then calls <paramref name="onDone"/>; ignored while a
    /// transition plays
    /// </summary>
    public static void SwitchTo(GameEdition edition, System.Action onDone = null)
    {
        if (switching || edition == Current || SceneTransition.IsPlaying) return;
        switching = true;
        SceneTransition.Play(() => Set(edition), () => {
            switching = false;
            onDone?.Invoke();
        });
    }

    /// <summary>
    /// Switches to the other edition (the SwitchEdition key)
    /// </summary>
    public static void Toggle() => SwitchTo(IsClassic ? GameEdition.Anniversary : GameEdition.Classic);

    /// <summary>
    /// Saves the edition and applies it at once; <see cref="SwitchTo"/> does it behind a wipe
    /// </summary>
    public static void Set(GameEdition edition)
    {
        if (edition == Current) return;
        current = edition;
        PlayerPrefs.SetInt(Key, (int)edition);
        PlayerPrefs.Save();
        ApplyPipeline();
        EditionMaterials.ApplyToLoadedScenes();
        VFXWarmup.Rewarm();
        Changed?.Invoke(edition);
    }

#if UNITY_EDITOR
    // The pipeline of the quality level is a project setting: the editor gets its own back when Play mode ends
    private static bool pipelineSaved;
    private static RenderPipelineAsset savedPipeline;

    private static void RestorePipeline(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode) return;
        QualitySettings.renderPipeline = savedPipeline;
        pipelineSaved = false;
        EditorApplication.playModeStateChanged -= RestorePipeline;
    }
#endif

    private static void ApplyPipeline()
    {
        RenderPipelineAsset pipeline = Profile.renderPipeline;
        if (pipeline == null || QualitySettings.renderPipeline == pipeline) return;
#if UNITY_EDITOR
        if (!pipelineSaved)
        {
            pipelineSaved = true;
            savedPipeline = QualitySettings.renderPipeline;
            EditorApplication.playModeStateChanged += RestorePipeline;
        }
#endif
        QualitySettings.renderPipeline = pipeline;
    }
}
