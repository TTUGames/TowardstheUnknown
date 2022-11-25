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
        if (Input.GetMouseButtonDown(0) && hoveredTile != null && hoveredTile.isSelectable && !ActionManager.IsBusy) {
            tileClicked.Invoke(hoveredTile);
        }
    }
}
