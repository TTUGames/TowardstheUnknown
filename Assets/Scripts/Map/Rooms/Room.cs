using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerDeploy))]
public class Room : MonoBehaviour
{
    public RoomType type;

    /// <summary>
    /// Fired when the pointer moves to another tile of the current room, with null when it leaves the tiles
    /// </summary>
    public static event System.Action<Tile> TileHovered;

    /// <summary>
    /// Fired when a selectable tile of the current room is clicked
    /// </summary>
    public static event System.Action<Tile> TileClicked;

    /// <summary>
    /// The tile of the current room under the pointer, null if none
    /// </summary>
    public static Tile HoveredTile { get; private set; }

    [SerializeField] private List<GameObject> lTilePossible;

    private RoomInfo roomInfo;

    private readonly List<TransitionTile> exits = new List<TransitionTile>();

    public IReadOnlyList<TransitionTile> Exits => exits;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        TileHovered = null;
        TileClicked = null;
        HoveredTile = null;
    }

    private void Awake() {
        exits.AddRange(GetComponentsInChildren<TransitionTile>());
        ReloadTilesWithRandomPrefab();
    }

    /// <summary>
    /// Removes the exits leading nowhere and adds the VFX shown on the others once they open
    /// </summary>
    public void SetExits(System.Func<Direction, bool> hasExit, GameObject exitVFX) {
        foreach (TransitionTile exit in exits) {
            if (hasExit(exit.direction)) exit.AddVFX(exitVFX);
            else {
                exit.tag = "Tile";
                DestroyImmediate(exit);
            }
		}
        exits.RemoveAll(exit => exit == null);
	}

    /// <summary>
    /// Initializes this room.
    /// Registers the player and the enemies in the turn system, and spawns the remaining loot
    /// </summary>
    /// <param name="info">The room's info. If its layout index is -1, does not load any spawnLayout</param>
    public void Init(RoomInfo info) {
        roomInfo = info;
        TurnSystem turnSystem = TurnSystem.Instance;
        turnSystem.Clear();
        PlayerTurn player = GameScene.Player;
        turnSystem.RegisterPlayer(player);

        if (info.GetLayoutIndex() != -1)
            GetComponentsInChildren<SpawnLayout>()[info.GetLayoutIndex()].Spawn();

        if (info.remainingOrbLoot != null)
            GetComponentInChildren<TreasureSpawnPoint>().Spawn(info.remainingOrbLoot);

        turnSystem.NotifyTurnOrderChanged();
        //Before they first play, so that the first show of an effect does not freeze the game
        VFXWarmup.Warm(GetComponentsInChildren<EnemyAI>().SelectMany(enemy => enemy.AllPatterns));
        VFXWarmup.Warm(player.Inventory.GetPlayerArtifacts());
        VFXWarmup.Warm(new[] { GameAssets.Instance.hit });
        GameEvents.EnterRoom(this, !info.IsAlreadyVisited());
    }

    private void OnEnable() {
        GameInput.Controls.Gameplay.Select.performed += OnSelect;
        GameEvents.CombatStarted += LockExits;
        GameEvents.CombatEnded += SpawnReward;
        GameEvents.ExplorationStarted += UnlockExits;
    }

    private void OnDisable() {
        GameInput.Controls.Gameplay.Select.performed -= OnSelect;
        GameEvents.CombatStarted -= LockExits;
        GameEvents.CombatEnded -= SpawnReward;
        GameEvents.ExplorationStarted -= UnlockExits;
        //The room is disabled when the player leaves it
        HoveredTile = null;
    }

	/// <summary>
    /// Updates the hovered tile. Done every frame: the hover highlight must be restored after tiles are reset.
    /// </summary>
	void Update()
    {
        Tile hovered = Tile.GetHoveredTile();
        if (hovered == HoveredTile) return;
        HoveredTile = hovered;
        TileHovered?.Invoke(hovered);
    }

    private void OnSelect(InputAction.CallbackContext context) {
        if (HoveredTile != null && HoveredTile.Selection != Tile.SelectionType.NONE)
            TileClicked?.Invoke(HoveredTile);
    }

    private void LockExits() => SetExitsOpen(false);

    private void UnlockExits() => SetExitsOpen(true);

    private void SetExitsOpen(bool open) {
        foreach (TransitionTile exit in exits)
            exit.SetOpen(open);
	}

    private void ReloadTilesWithRandomPrefab() {
        //The exits, tagged MapChangerTile, keep their model
        foreach (Tile tile in GetComponentsInChildren<Tile>()) {
            if (!tile.CompareTag("Tile")) continue;
            tile.GetComponent<MeshFilter>().sharedMesh = lTilePossible[Random.Range(0, lTilePossible.Count)].GetComponent<MeshFilter>().sharedMesh;
            tile.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            tile.FindNeighbors();
        }
    }

    /// <summary>
    /// On combat end, spawns a reward
    /// </summary>
    private void SpawnReward() {
        TreasureSpawnPoint rewardSpawnPoint = GetComponentInChildren<TreasureSpawnPoint>();
        if (rewardSpawnPoint != null)
            rewardSpawnPoint.Spawn();
    }

    /// <summary>
    /// On destroy, registers the collectable in the RoomInfo to load them again next time this room is entered
    /// </summary>
    private void OnDestroy() {
        if (roomInfo == null) return;
        Collectable remainingCollectable = GetComponentInChildren<Collectable>();
        roomInfo.remainingOrbLoot = remainingCollectable != null ? remainingCollectable.GetArtifacts() : null;
	}
}
