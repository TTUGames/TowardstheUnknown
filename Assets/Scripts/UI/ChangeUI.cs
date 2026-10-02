using UnityEngine;
using UnityEngine.InputSystem;
using Steamworks;

/// <summary>
/// Gives access to the in game UI (HUD, inventory, pause and results) and opens its menus from the input
/// </summary>
public class ChangeUI : MonoBehaviour
{
    public UIPause uIPause;
    [SerializeField] private Hud hud;
    [SerializeField] private InventoryScreen inventory;

    public SlantedWipe Fade => hud.Fade;
    public MinimapPanel Minimap => hud.Minimap;
    public Hud Hud => hud;
    public InventoryScreen Inventory => inventory;
    private Results results;
    private Results Results => results != null ? results : results = GetComponent<Results>();

    /// <summary>
    /// Checks if a menu covering the game is open
    /// </summary>
    public bool IsMenuOpen => uIPause.IsPaused || inventory.IsOpen || Results.IsShown;

    /// <summary>
    /// Fired when a menu covering the game opens or closes: read <see cref="IsMenuOpen"/>
    /// </summary>
    public event System.Action MenuChanged;

    /// <summary>
    /// Called by the menus once they opened or closed
    /// </summary>
    public void NotifyMenuChanged() => MenuChanged?.Invoke();

    private Callback<GameOverlayActivated_t> overlayCallback;

    private void OnEnable()
    {
        GameInput.Controls.Menus.ToggleInventory.performed += OnToggleInventory;
        GameInput.Controls.Menus.Back.performed += OnBack;
        if (SteamManager.Initialized) overlayCallback = Callback<GameOverlayActivated_t>.Create(OnOverlay);
    }

    private void OnDisable()
    {
        GameInput.Controls.Menus.ToggleInventory.performed -= OnToggleInventory;
        GameInput.Controls.Menus.Back.performed -= OnBack;
        overlayCallback?.Dispose();
        overlayCallback = null;
    }

    // Leaving the game (Alt+Tab, the Steam overlay) opens the pause: the game runs in the background, the enemy turns
    // would go on unseen. Not in the editor, whose game view loses the focus to every other window
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !Application.isEditor) PauseFromOutside();
    }

    private void OnOverlay(GameOverlayActivated_t overlay)
    {
        if (overlay.m_bActive != 0) PauseFromOutside();
    }

    /// <summary>
    /// Opens the pause unless a menu that stops the game is shown (the pause, the results) or the player is dead
    /// </summary>
    public void PauseFromOutside()
    {
        PlayerTurn player = GameScene.Player;
        if (player != null && player.Stats.CurrentHealth > 0 && !Results.IsShown && !uIPause.IsPaused)
            uIPause.ToggleOptions(true);
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (!uIPause.IsPaused && !Results.IsShown)
            inventory.Toggle();
    }

    private void OnBack(InputAction.CallbackContext context)
    {
        PlayerTurn player = GameScene.Player;
        if (player != null && player.Stats.CurrentHealth > 0 && !Results.IsShown)
            uIPause.ChangeStateOptions();
    }
}
