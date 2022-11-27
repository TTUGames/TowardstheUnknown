using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Room))]
public class PlayerDeploy : MonoBehaviour
{
    public List<Tile> deployTiles; //Editable in inspector
    private float playerSpawnYPosition = 0.5f;
    private Transform player;

    [HideInInspector] bool isDone = false;

    private Room room;

    private Tile targetTile = null;

	private void Awake() {
        room = GetComponent<Room>();
	}

    /// <summary>
    /// Deploys the player in the room.
    /// If enemies are present, gives the choice between all deployTiles.
    /// Else, deploys the protagonist on the transitionTile corresponding to the room he comes from.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromDirection"></param>
    /// <returns></returns>
	public IEnumerator DeployPlayer(Transform player, Direction fromDirection) {
        this.player = player;

        if (GetComponentInChildren<EnemyStats>() != null) { //The room contains enemies
            foreach (Tile deployTile in deployTiles) {
                deployTile.selectionType = Tile.SelectionType.DEPLOY;
            }

            player.position = deployTiles[0].transform.position + Vector3.up * playerSpawnYPosition;

            room.newTileHovered.AddListener(OnDeployTileHovered);
            room.tileClicked.AddListener(OnDeployTileClick);

            yield return new WaitUntil(() => isDone);
            EndDeployPhase();
        }
        else { //The room is empty, there is no need to deploy the player on specific tiles
            Tile deployTile = null;
            foreach (TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>()) {
                if (transitionTile.direction == fromDirection) deployTile = transitionTile.GetComponent<Tile>();
			}
            MovePlayerToTile(deployTile);
            isDone = true;
		}
        
    }

    /// <summary>
    /// In the deploy phase, hovering a deploy tile moves the character and sets the tile color
    /// </summary>
    /// <param name="tile"></param>
    private void OnDeployTileHovered(Tile tile) {
        if (targetTile != null) targetTile.isTarget = false;
        if (tile == null || !deployTiles.Contains(tile)) return;
        tile.isTarget = true;
        MovePlayerToTile(tile);
        targetTile = tile;
    }

    /// <summary>
    /// In the deploy phase, clicking a tile moves the character and ends the phase
    /// </summary>
    /// <param name="tile"></param>
    private void OnDeployTileClick(Tile tile) {
        if (!deployTiles.Contains(tile)) return;
        MovePlayerToTile(tile);

        isDone = true;
    }

    /// <summary>
    /// Cleans the events and selected tiles
    /// </summary>
    private void EndDeployPhase() {
        targetTile.isTarget = false;

        room.newTileHovered.RemoveListener(OnDeployTileHovered);
        room.tileClicked.RemoveListener(OnDeployTileClick);

    }

    /// <summary>
    /// Moves the player to target tile
    /// </summary>
    /// <param name="tile"></param>
    private void MovePlayerToTile(Tile tile) {
        player.position = tile.transform.position + Vector3.up * playerSpawnYPosition;
    }
}
