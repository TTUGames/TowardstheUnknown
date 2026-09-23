using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    private void Awake() {
        currentRoom = this;
        ReloadTilesWithRandomPrefab();
    }

    /// <summary>
    /// Disables this room's exits depending on the parameters
    /// </summary>
    public void SetExits(bool hasNorthExit, bool hasSouthExit, bool hasEastExit, bool hasWestExit) {
        foreach (TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>()) {
            bool hasExit = transitionTile.direction switch {
                Direction.NORTH => hasNorthExit,
                Direction.SOUTH => hasSouthExit,
                Direction.EAST => hasEastExit,
                Direction.WEST => hasWestExit,
                _ => true,
            };
            if (!hasExit) {
                transitionTile.tag = "Tile";
                DestroyImmediate(transitionTile);
            }
		}
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
        turnSystem.RegisterPlayer(FindAnyObjectByType<PlayerTurn>());

        if (!info.IsAlreadyVisited()) {
            if (type != RoomType.SPAWN) {
                PlayerInfo playerInfo = FindAnyObjectByType<PlayerInfo>();
                if (playerInfo != null) playerInfo.visitedRoomCount += 1;
                SteamAchievements.IncrementStat("explored_rooms", 1);
            }
            FindAnyObjectByType<PlayerStats>().OnFirstTimeRoomEnter(this);
        }

        if (info.GetLayoutIndex() != -1)
            GetComponentsInChildren<SpawnLayout>()[info.GetLayoutIndex()].Spawn();

        if (info.remainingOrbLoot != null)
            GetComponentInChildren<TreasureSpawnPoint>().Spawn(info.remainingOrbLoot);

        TimelineManager timelineManager = FindAnyObjectByType<TimelineManager>();
        if (timelineManager != null)
            timelineManager.UpdateTimeline();
    }

	/// <summary>
    /// Checks if a tile is hovered or clicked, and calls the relevant functions.
    /// </summary>
	void Update()
    {
        Tile previousHoveredTile = hoveredTile;
        hoveredTile = Tile.GetHoveredTile();
        if (hoveredTile != previousHoveredTile)
            newTileHovered.Invoke(hoveredTile);
        if (Input.GetMouseButtonDown(0) && hoveredTile != null && hoveredTile.Selection != Tile.SelectionType.NONE)
            tileClicked.Invoke(hoveredTile);
    }

    public void LockExits(bool lockState) {
        foreach (TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>())
            transitionTile.vfx.SetActive(!lockState);
	}

    private void ReloadTilesWithRandomPrefab() {
        foreach (GameObject tile in GameObject.FindGameObjectsWithTag("Tile")) {
            tile.GetComponent<MeshFilter>().sharedMesh = lTilePossible[Random.Range(0, lTilePossible.Count)].GetComponent<MeshFilter>().sharedMesh;
            tile.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            tile.GetComponent<Tile>().FindNeighbors();
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
