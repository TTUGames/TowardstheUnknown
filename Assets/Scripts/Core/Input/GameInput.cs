using UnityEngine;

/// <summary>
/// Owns the game's <c>Controls</c>, enabled for the whole application
/// </summary>
public static class GameInput
{
    private static Controls controls;

    // Created before the first scene loads, or again after a script reload in Play Mode, which resets the static fields
    public static Controls Controls => controls ??= Create();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() {
        controls?.Dispose();
        controls = Create();
    }

    private static Controls Create() {
        var created = new Controls();
        created.Gameplay.Enable();
        created.Inventory.Enable();
        created.Menus.Enable();
        //Debug shortcuts are never available in release builds
        if (Debug.isDebugBuild) created.Debug.Enable();
        return created;
    }

    /// <summary>
    /// The pointer's position on the screen, in pixels
    /// </summary>
    public static Vector2 PointerPosition => Controls.Gameplay.Point.ReadValue<Vector2>();
}
