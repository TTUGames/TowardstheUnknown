using UnityEngine;

/// <summary>
/// Moves between the build scenes: 0 is the pre-menu, 1 the main menu and 2 the game.
/// The scenes load in the background behind a wipe (<see cref="SceneTransition"/>)
/// </summary>
public static class GameFlow
{
    public const int MainMenuScene = 1;
    public const int GameScene = 2;

    /// <summary>
    /// True from the start of a scene change until the new scene is shown: further requests are ignored
    /// </summary>
    public static bool IsLoading { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        IsLoading = false;
    }

    // The version heads the Player.log, for the bug reports
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LogVersion() {
        Debug.Log($"{Application.productName} v{Application.version} (Unity {Application.unityVersion}, {Application.platform})");
    }

    public static void LoadMainMenu() => Load(MainMenuScene);

    /// <summary>
    /// A new run: the suspended one is dropped
    /// </summary>
    public static void StartRun()
    {
        if (IsLoading) return;
        RunSave.StartNew();
        Load(GameScene);
    }

    /// <summary>
    /// Goes on with the suspended run (<see cref="RunSave"/>), if it can be read
    /// </summary>
    public static void ContinueRun()
    {
        if (!IsLoading && RunSave.Resume()) Load(GameScene);
    }

    public static void Quit() => Application.Quit();

    private static void Load(int sceneIndex)
    {
        if (IsLoading) return;
        IsLoading = true;
        //Leaving from the pause menu, or during a slow motion
        GameTime.Paused = false;
        GameTime.Clear();
        SceneTransition.Play(sceneIndex, () => IsLoading = false);
    }
}
