using System.Linq;
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
    private Tile previousHoveredTile;

    private TurnSystem turnSystem;

    private List<TransitionTile> transitionTiles;

    [SerializeField] private List<GameObject> lTilePossible;

    private RoomInfo roomInfo;


    private void Awake() {
        currentRoom = this;
        turnSystem = GameObject.Find("Gameplay").GetComponent<TurnSystem>();
        ReloadTilesWithRandomPrefab();

        RoomInfosDisplay roomInfosDisplay = Object.FindObjectOfType<RoomInfosDisplay>();
        if (roomInfosDisplay != null)
            roomInfosDisplay.UpdateText();
    }

    /// <summary>
    /// Disables this room's exits depending on the parameters
    /// </summary>
    /// <param name="hasNorthExit"></param>
    /// <param name="hasSouthExit"></param>
    /// <param name="hasWestExit"></param>
    /// <param name="hasEastExit"></param>
    public void SetExits(bool hasNorthExit, bool hasSouthExit, bool hasWestExit, bool hasEastExit) {
        foreach (TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>()) {
            if (transitionTile.direction == Direction.NORTH && !hasNorthExit ||
                    transitionTile.direction == Direction.SOUTH && !hasSouthExit ||
                    transitionTile.direction == Direction.EAST && !hasEastExit ||
                    transitionTile.direction == Direction.WEST && !hasWestExit) {
                transitionTile.tag = "Tile";
                DestroyImmediate(transitionTile);
            }
		}
	}

    /// <summary>
    /// Initializes this room. 
    /// Registers the player and the enemies in the turn system, starts the deploy phase, then starts combat or exploration
    /// </summary>
    /// <param name="layoutIndex">The layout index to be used. If not set or set to -1, does not load any spawnLayout</param>
    /// <returns></returns>
    public void Init(RoomInfo info) {
        roomInfo = info;
        turnSystem.Clear();

        turnSystem.RegisterPlayer(FindObjectOfType<PlayerTurn>());

        if (!info.IsAlreadyVisited()) {
            if (type != RoomType.SPAWN) {
                PlayerInfo playerInfo = GameObject.FindObjectOfType<PlayerInfo>();
                if (playerInfo != null) playerInfo.visitedRoomCount += 1;
                SteamAchievements.IncrementStat("explored_rooms", 1);
            }
            FindObjectOfType<PlayerStats>().OnFirstTimeRoomEnter(this);
        }

        if (info.GetLayoutIndex() != -1) {
            List<SpawnLayout> spawnLayouts = new List<SpawnLayout>(GetComponentsInChildren<SpawnLayout>());
            SpawnLayout chosenSpawnLayout = spawnLayouts[info.GetLayoutIndex()];

            chosenSpawnLayout.Spawn();
        }

        if (info.remainingOrbLoot != null) {
            TreasureSpawnPoint spawnPoint = GetComponentInChildren<TreasureSpawnPoint>();
            spawnPoint.Spawn(info.remainingOrbLoot);
		}

        TimelineManager timelineManager = Object.FindObjectOfType<TimelineManager>();
        if (timelineManager != null)
            timelineManager.UpdateTimeline();
    }

	/// <summary>
    /// Checks if a tile is hovered or clicked, and calls the relevant functions.
    /// </summary>
	void Update()
    {
        previousHoveredTile = hoveredTile;
        hoveredTile = Tile.GetHoveredTile();
        if (hoveredTile != previousHoveredTile) {
            newTileHovered.Invoke(hoveredTile);
        }
        if (Input.GetMouseButtonDown(0) && hoveredTile != null && hoveredTile.Selection != Tile.SelectionType.NONE && !ActionManager.IsBusy) {
            tileClicked.Invoke(hoveredTile);
        }
    }

    public void LockExits(bool lockState) {
        foreach(TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>()) {
            //transitionTile.GetComponent<Tile>().isWalkable = !lockState;
            transitionTile.vfx.SetActive(!lockState);
		}
	}

    private void ReloadTilesWithRandomPrefab() {
        GameObject.FindGameObjectsWithTag("Tile").ToList().ForEach(tile =>
        {
            int randomIndexTexture = Random.Range(0, lTilePossible.Count);
            tile.GetComponent<MeshFilter>().sharedMesh = lTilePossible[randomIndexTexture].GetComponent<MeshFilter>().sharedMesh;
            int randomIndexRotation = Random.Range(0, 4);
            tile.transform.rotation = Quaternion.Euler(0, 90 * randomIndexRotation, 0);
            tile.GetComponent<Tile>().FindNeighbors();
        });
    }

    /// <summary>
    /// On combat end, unlocks the exits and spawns a reward
    /// </summary>
    public void OnRoomClear() {
        LockExits(false);
        SpawnPoint rewardSpawnPoint = GetComponentInChildren<TreasureSpawnPoint>();
        if (rewardSpawnPoint != null)
            rewardSpawnPoint.Spawn();
    }

    /// <summary>
    /// On destroy, registers the collectable in the RoomInfo to load them again next time this room is entered
    /// </summary>
    private void OnDestroy() {
        Collectable remainingCollectable = GetComponentInChildren<Collectable>();
        if (remainingCollectable != null) roomInfo.remainingOrbLoot = remainingCollectable.GetArtifacts();
        else roomInfo.remainingOrbLoot = null;
	}

    /// <summary>
    /// Heals the player on room enter
    /// </summary>
    private void ApplyHeal() {
        if (type == RoomType.ANTECHAMBER) {
            PlayerStats player = FindObjectOfType<PlayerStats>();
            player.Heal(Mathf.FloorToInt((player.MaxHealth - player.CurrentHealth) * 0.5f));
        }
    }
}
