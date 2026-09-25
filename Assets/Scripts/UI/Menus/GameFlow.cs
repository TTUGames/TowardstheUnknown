using UnityEngine;

/// <summary>
/// Moves between the build scenes: 0 is the pre-menu, 1 the main menu and 2 the game.
/// The scenes load in the background behind a fade to black (<see cref="SceneTransition"/>)
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

    public static void LoadMainMenu() => Load(MainMenuScene);

    public static void StartRun() => Load(GameScene);

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
