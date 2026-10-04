using UnityEngine;
using UnityEngine.InputSystem;

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

    private const string BindingsKey = "Bindings";

    private static Controls Create() {
        var created = new Controls();
        //The keys the player rebound in the options (ControlsPage)
        string bindings = PlayerPrefs.GetString(BindingsKey, "");
        if (bindings.Length > 0) created.asset.LoadBindingOverridesFromJson(bindings);
        created.Gameplay.Enable();
        created.Inventory.Enable();
        created.Menus.Enable();
        //Debug shortcuts are never available in release builds
        if (Debug.isDebugBuild) created.Debug.Enable();
        return created;
    }

    /// <summary>
    /// Saves the keys rebound in the options
    /// </summary>
    public static void SaveBindings() {
        PlayerPrefs.SetString(BindingsKey, Controls.asset.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Gives back the default keys
    /// </summary>
    public static void ResetBindings() {
        Controls.asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(BindingsKey);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// The pointer's position on the screen, in pixels
    /// </summary>
    public static Vector2 PointerPosition => Controls.Gameplay.Point.ReadValue<Vector2>();
}
