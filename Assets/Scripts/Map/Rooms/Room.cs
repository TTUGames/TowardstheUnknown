using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerDeploy))]
public class Room : MonoBehaviour
{
    public static Room currentRoom;

    public RoomType type;

    [HideInInspector] public UnityEvent<Tile> newTileHovered = new UnityEvent<Tile>();
    [HideInInspector] public UnityEvent<Tile> tileClicked = new UnityEvent<Tile>();

    [HideInInspector] public Tile hoveredTile;

    [SerializeField] private List<GameObject> lTilePossible;

    private RoomInfo roomInfo;

    private readonly List<TransitionTile> exits = new List<TransitionTile>();

    public IReadOnlyList<TransitionTile> Exits => exits;

    private void Awake() {
        currentRoom = this;
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

        if (!info.IsAlreadyVisited()) {
            if (type != RoomType.SPAWN) {
                GameScene.UI.PlayerInfo.visitedRoomCount += 1;
                SteamAchievements.IncrementStat("explored_rooms", 1);
            }
            player.Stats.OnFirstTimeRoomEnter(this);
        }

        if (info.GetLayoutIndex() != -1)
            GetComponentsInChildren<SpawnLayout>()[info.GetLayoutIndex()].Spawn();

        if (info.remainingOrbLoot != null)
            GetComponentInChildren<TreasureSpawnPoint>().Spawn(info.remainingOrbLoot);

        turnSystem.NotifyTurnOrderChanged();
    }

    private void OnEnable() {
        GameInput.Controls.Gameplay.Select.performed += OnSelect;
    }

    private void OnDisable() {
        GameInput.Controls.Gameplay.Select.performed -= OnSelect;
    }

	/// <summary>
    /// Updates the hovered tile. Done every frame: the hover highlight must be restored after tiles are reset.
    /// </summary>
	void Update()
    {
        Tile previousHoveredTile = hoveredTile;
        hoveredTile = Tile.GetHoveredTile();
        if (hoveredTile != previousHoveredTile)
            newTileHovered.Invoke(hoveredTile);
    }

    private void OnSelect(InputAction.CallbackContext context) {
        if (hoveredTile != null && hoveredTile.Selection != Tile.SelectionType.NONE)
            tileClicked.Invoke(hoveredTile);
    }

    public void LockExits(bool lockState) {
        foreach (TransitionTile exit in exits)
            exit.SetOpen(!lockState);
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
    /// On combat end, unlocks the exits and spawns a reward
    /// </summary>
    public void OnRoomClear() {
        LockExits(false);
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
