using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        OnRoomEnter();
	}

    public void SetExits(bool hasNorthExit, bool hasSouthExit, bool hasWestExit, bool hasEastExit) {
        Debug.Log("NORTH : " + hasNorthExit);
        Debug.Log("SOUTH : " + hasSouthExit);
        Debug.Log("WEST : " + hasWestExit);
        Debug.Log("EAST : " + hasEastExit);
        foreach (TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>()) {
            switch(transitionTile.direction) {
                case Direction.NORTH:
                    if (!hasNorthExit) {
                        transitionTile.tag = "Tile";
                        DestroyImmediate(transitionTile);
                    }
                    break;
                case Direction.SOUTH:
                    if (!hasSouthExit) {
                        transitionTile.tag = "Tile";
                        DestroyImmediate(transitionTile);
                    }
                    break;
                case Direction.EAST:
                    if (!hasEastExit) {
                        transitionTile.tag = "Tile";
                        DestroyImmediate(transitionTile);
                    }
                    break;
                case Direction.WEST:
                    if (!hasWestExit) {
                        transitionTile.tag = "Tile";
                        DestroyImmediate(transitionTile);
                    }
                    break;
			}
		}
	}

    private void OnRoomEnter() {
        turnSystem.Clear();
        turnSystem.RegisterPlayer(FindObjectOfType<PlayerTurn>());

        List<GameObject> spawnLayouts = new List<GameObject>();
        foreach(Transform spawnLayout in transform.Find("SpawnLayouts")) {
            spawnLayouts.Add(spawnLayout.gameObject);
		}
        GameObject chosenSpawnLayout = spawnLayouts[Random.Range(0, spawnLayouts.Count)];

        foreach(SpawnPoint spawnPoint in chosenSpawnLayout.GetComponentsInChildren<SpawnPoint>()) {
            turnSystem.RegisterEnemy(spawnPoint.SpawnEntity());
		}
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
