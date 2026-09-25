using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Moves between the build scenes: 0 is the pre-menu, 1 the main menu and 2 the game
/// </summary>
public static class GameFlow
{
    public const int MainMenuScene = 1;
    public const int GameScene = 2;

    public static void LoadMainMenu() => SceneManager.LoadScene(MainMenuScene);

    public static void StartRun() => SceneManager.LoadScene(GameScene);

    public static void Quit() => Application.Quit();
}
