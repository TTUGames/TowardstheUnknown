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

	public IEnumerator DeployPlayer(Transform player) {
        this.player = player;

        foreach (Tile deployTile in deployTiles) {
            deployTile.selectionType = Tile.SelectionType.DEPLOY;
        }

        player.position = deployTiles[0].transform.position + Vector3.up * playerSpawnYPosition;

        room.newTileHovered.AddListener(OnDeployTileHovered);
        room.tileClicked.AddListener(OnDeployTileClick);

        yield return new WaitUntil(() => isDone);
        EndDeployPhase();
    }

    private void OnDeployTileHovered(Tile tile) {
        if (targetTile != null) targetTile.isTarget = false;
        if (tile == null || !deployTiles.Contains(tile)) return;
        tile.isTarget = true;
        MovePlayerToTile(tile);
        targetTile = tile;
    }

    private void OnDeployTileClick(Tile tile) {
        if (!deployTiles.Contains(tile)) return;
        MovePlayerToTile(tile);

        isDone = true;
    }

    private void EndDeployPhase() {
        targetTile.isTarget = false;

        room.newTileHovered.RemoveListener(OnDeployTileHovered);
        room.tileClicked.RemoveListener(OnDeployTileClick);

    }

    private void MovePlayerToTile(Tile tile) {
        player.position = tile.transform.position + Vector3.up * playerSpawnYPosition;
    }
}
