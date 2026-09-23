using UnityEngine;

/// <summary>
/// Owns the game's <c>Controls</c>, enabled for the whole application
/// </summary>
public static class GameInput
{
    public static Controls Controls { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() {
        Controls?.Dispose();
        Controls = new Controls();
        Controls.Gameplay.Enable();
        Controls.Inventory.Enable();
        Controls.Menus.Enable();
        //Debug shortcuts are never available in release builds
        if (Debug.isDebugBuild) Controls.Debug.Enable();
    }

    /// <summary>
    /// The pointer's position on the screen, in pixels
    /// </summary>
    public static Vector2 PointerPosition => Controls.Gameplay.Point.ReadValue<Vector2>();
}
