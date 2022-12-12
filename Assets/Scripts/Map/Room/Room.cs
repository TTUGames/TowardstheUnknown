using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerDeploy))]
public class Room : MonoBehaviour
{
    public static Room currentRoom;

    public UnityEvent<Tile> newTileHovered = new UnityEvent<Tile>();
    public UnityEvent<Tile> tileClicked = new UnityEvent<Tile>();

    public Tile hoveredTile;
    private Tile previousHoveredTile;

    private TurnSystem turnSystem;

    private List<TransitionTile> transitionTiles;

    [SerializeField] private List<GameObject> lTilePossible;


    private void Awake() {
        currentRoom = this;
        turnSystem = GameObject.Find("Gameplay").GetComponent<TurnSystem>();
        ReloadTilesWithRandomPrefab();
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
    public void Init(int layoutIndex) {
        turnSystem.Clear();

        turnSystem.RegisterPlayer(FindObjectOfType<PlayerTurn>());

        if (layoutIndex != -1) {
            List<SpawnLayout> spawnLayouts = new List<SpawnLayout>(GetComponentsInChildren<SpawnLayout>());
            SpawnLayout chosenSpawnLayout = spawnLayouts[layoutIndex];

            chosenSpawnLayout.Spawn();
        }
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
            transitionTile.GetComponent<Tile>().isWalkable = !lockState;
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
}
