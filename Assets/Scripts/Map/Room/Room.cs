using System.Collections;
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

	private void Start() {
        currentRoom = this;
        turnSystem = GameObject.Find("Gameplay").GetComponent<TurnSystem>();
        StartCoroutine(OnRoomEnter());
	}

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

    private IEnumerator OnRoomEnter() {
        turnSystem.Clear();

        turnSystem.RegisterPlayer(FindObjectOfType<PlayerTurn>());

        List<GameObject> spawnLayouts = new List<GameObject>();
        foreach (Transform spawnLayout in transform.Find("SpawnLayouts")) {
            spawnLayouts.Add(spawnLayout.gameObject);
        }
        GameObject chosenSpawnLayout = spawnLayouts[Random.Range(0, spawnLayouts.Count)];

        foreach (SpawnPoint spawnPoint in chosenSpawnLayout.GetComponentsInChildren<SpawnPoint>()) {
            turnSystem.RegisterEnemy(spawnPoint.SpawnEntity());
        }

        yield return GetComponent<PlayerDeploy>().DeployPlayer(FindObjectOfType<PlayerTurn>().transform);

        turnSystem.CheckForCombatStart();
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
        if (Input.GetMouseButtonDown(0) && hoveredTile != null && hoveredTile.selectionType != Tile.SelectionType.NONE && !ActionManager.IsBusy) {
            tileClicked.Invoke(hoveredTile);
        }
    }
}
