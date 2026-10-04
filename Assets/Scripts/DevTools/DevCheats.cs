using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// The shortcuts of a run in the editor and development builds (the Debug controls are never enabled in release builds):
/// F6 makes the player invincible (never under 1 health), F7 reveals the whole minimap, F8 goes to Drareg's room out of
/// combat, F9 shows the frame rate, the version, the edition and the map's seed in a corner. Made at the first scene and kept
/// </summary>
public class DevCheats : MonoBehaviour
{
    // Over the game's documents
    private const int SortingOrder = 900;
    private const float OverlayInterval = 0.5f;

    private UIDocument document;
    private Label overlay;
    private float overlayTime;
    private int overlayFrames;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (!Debug.isDebugBuild) return;
        // Inactive until configured: the UIDocument joins its panel in OnEnable
        var go = new GameObject(nameof(DevCheats));
        go.SetActive(false);
        DontDestroyOnLoad(go);
        var cheats = go.AddComponent<DevCheats>();
        cheats.document = go.AddComponent<UIDocument>();
        cheats.document.panelSettings = GameAssets.Instance.panelSettings;
        cheats.document.sortingOrder = SortingOrder;
        go.SetActive(true);
    }

    private void Start()
    {
        VisualElement root = document.rootVisualElement;
        root.pickingMode = PickingMode.Ignore;
        overlay = new Label { pickingMode = PickingMode.Ignore };
        overlay.AddToClassList("dev-overlay");
        overlay.AddToClassList("hidden");
        root.Add(overlay);
        Letterbox.Fit(root);
    }

    private void OnEnable()
    {
        var debug = GameInput.Controls.Debug;
        debug.Invincible.performed += OnInvincible;
        debug.RevealMap.performed += OnRevealMap;
        debug.GoToBoss.performed += OnGoToBoss;
        debug.DevOverlay.performed += OnDevOverlay;
    }

    private void OnDisable()
    {
        var debug = GameInput.Controls.Debug;
        debug.Invincible.performed -= OnInvincible;
        debug.RevealMap.performed -= OnRevealMap;
        debug.GoToBoss.performed -= OnGoToBoss;
        debug.DevOverlay.performed -= OnDevOverlay;
    }

    private void OnInvincible(InputAction.CallbackContext context)
    {
        if (GameScene.Map == null) return;
        EntityStats stats = GameScene.Player.Stats;
        stats.Immortal = !stats.Immortal;
        Debug.Log("Dev - Invincible: " + stats.Immortal);
    }

    private void OnRevealMap(InputAction.CallbackContext context)
    {
        if (GameScene.Map == null) return;
        GameScene.UI.Minimap.RevealAll();
        Debug.Log("Dev - Map revealed");
    }

    private void OnGoToBoss(InputAction.CallbackContext context)
    {
        Map map = GameScene.Map;
        if (map == null || map.CurrentRoom == null || TurnSystem.Instance.IsCombat) return;
        for (int x = 0; x < map.Rooms.Count; x++)
            for (int y = 0; y < map.Rooms[x].Count; y++)
                if (map.Rooms[x][y] != null && map.Rooms[x][y].GetRoomType() == RoomType.BOSS)
                {
                    Debug.Log($"Dev - Going to the boss room ({x}, {y})");
                    map.JumpTo(new Vector2Int(x, y));
                    return;
                }
    }

    private void OnDevOverlay(InputAction.CallbackContext context)
    {
        if (overlay == null) return;
        overlay.ToggleInClassList("hidden");
        overlayTime = 0;
        overlayFrames = 0;
    }

    private void Update()
    {
        if (overlay == null || overlay.ClassListContains("hidden")) return;
        overlayTime += Time.unscaledDeltaTime;
        overlayFrames++;
        if (overlayTime < OverlayInterval) return;
        string seed = GameScene.Map != null && GameScene.Map.Seed != 0 ? $" · seed {GameScene.Map.Seed}" : "";
        overlay.text = $"{overlayFrames / overlayTime:0} FPS · v{Application.version} · {Edition.Current}{seed}";
        overlayTime = 0;
        overlayFrames = 0;
    }
}
