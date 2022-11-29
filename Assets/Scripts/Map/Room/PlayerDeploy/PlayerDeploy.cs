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

    /// <summary>
    /// Deploys the player on a tile.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromDirection"></param>
    /// <returns></returns>
    public abstract IEnumerator DeployPlayer(Transform player, Direction fromDirection);


    /// <summary>
    /// Moves the player to target tile
    /// </summary>
    /// <param name="tile"></param>
    protected void MovePlayerToTile(Transform player, Tile tile) {
        player.position = tile.transform.position + Vector3.up * playerSpawnYPosition;
    }

    /// <summary>
    /// Deploys the player if he comes from an adjacent room
    /// Deploys him on the tile adjacent to the transitionTile correponding to the room he comes from.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromDirection"></param>
    /// <exception cref="System.Exception"></exception>
    protected void DefaultDeploy(Transform player, Direction fromDirection) {
        Tile deployTile = null;
        foreach (TransitionTile transitionTile in GetComponentsInChildren<TransitionTile>()) {
            if (transitionTile.direction == fromDirection) deployTile = transitionTile.GetComponent<Tile>();
        }
        if (deployTile == null) throw new System.Exception("Cannot find valid tile to deploy");

        Vector3 playerDeployPosition = deployTile.transform.position + Vector3.up * playerSpawnYPosition;
        Vector2Int offset = DirectionConverter.DirToVect(DirectionConverter.GetOppositeDirection(fromDirection));

        playerDeployPosition.x += offset.x;
        playerDeployPosition.z += offset.y;
        
        player.position = playerDeployPosition;
    }
}
