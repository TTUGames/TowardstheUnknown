using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gives access to the in game UI (HUD, inventory, pause and results) and opens its menus from the input
/// </summary>
public class ChangeUI : MonoBehaviour
{
    public UIPause uIPause;
    [SerializeField] private Hud hud;
    [SerializeField] private InventoryScreen inventory;

    public PlayerInfo PlayerInfo => GetComponent<PlayerInfo>();
    public ScreenFade Fade => hud.Fade;
    public MinimapPanel Minimap => hud.Minimap;
    public Hud Hud => hud;
    public InventoryScreen Inventory => inventory;
    private Results Results => GetComponent<Results>();

    /// <summary>
    /// Checks if a menu covering the game is open
    /// </summary>
    public bool IsMenuOpen => uIPause.isPaused || inventory.IsOpen || Results.IsShown;

    private void OnEnable()
    {
        GameInput.Controls.Menus.ToggleInventory.performed += OnToggleInventory;
        GameInput.Controls.Menus.Back.performed += OnBack;
    }

    private void OnDisable()
    {
        GameInput.Controls.Menus.ToggleInventory.performed -= OnToggleInventory;
        GameInput.Controls.Menus.Back.performed -= OnBack;
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (!uIPause.isPaused && !Results.IsShown)
            inventory.Toggle();
    }

    private void OnBack(InputAction.CallbackContext context)
    {
        PlayerTurn player = GameScene.Player;
        if (player != null && player.Stats.CurrentHealth > 0 && !Results.IsShown)
            uIPause.ChangeStateOptions();
    }
}
