using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerDeploy : MonoBehaviour
{
    private float playerSpawnYPosition = 0.5f;

    [HideInInspector] bool isDone = false;

    private Room room;

    private void Awake() {
        room = GetComponent<Room>();
    }

    public abstract IEnumerator DeployPlayer(Transform player, Direction fromDirection);


    /// <summary>
    /// Moves the player to target tile
    /// </summary>
    /// <param name="tile"></param>
    protected void MovePlayerToTile(Transform player, Tile tile) {
        player.position = tile.transform.position + Vector3.up * playerSpawnYPosition;
    }

    protected void DefaultDeploy(Transform player, Direction fromDirection) {
        Tile deployTile = null;
        foreach (TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>()) {
            if (transitionTile.direction == fromDirection) deployTile = transitionTile.GetComponent<Tile>();
        }
        if (deployTile == null) throw new System.Exception("Cannot find valid tile to deploy");
        MovePlayerToTile(player, deployTile);
    }
}
