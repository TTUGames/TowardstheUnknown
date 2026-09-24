using System.Collections;
using UnityEngine;

public class PlayerDeploy : MonoBehaviour
{
    protected float playerSpawnYPosition = 0.5f;

    protected bool isDone = false;

    protected Room room;

    protected void Awake() {
        room = GetComponent<Room>();
    }

    /// <summary>
    /// Deploys the player on a tile.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromDirection"></param>
    /// <returns></returns>
    public virtual IEnumerator DeployPlayer(Transform player, Direction fromDirection) {
        DefaultDeploy(player, fromDirection);
        yield return null;
	}

    /// <summary>
    /// Moves the player to target tile
    /// </summary>
    protected void MovePlayerToTile(Transform player, Tile tile) {
        MovePlayerTo(player, tile.transform.position + Vector3.up * playerSpawnYPosition);
    }

    private void MovePlayerTo(Transform player, Vector3 position) {
        player.position = position;
        player.GetComponent<TacticsMove>().SetCurrentTileFromRaycast();
    }

    /// <summary>
    /// Deploys the player if he comes from an adjacent room
    /// Deploys him on the tile adjacent to the transitionTile correponding to the room he comes from.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromDirection"></param>
    /// <exception cref="System.Exception"></exception>
    protected void DefaultDeploy(Transform player, Direction fromDirection) {
        TransitionTile entrance = null;
        foreach (TransitionTile exit in room.Exits) {
            if (exit.direction == fromDirection) entrance = exit;
        }
        if (entrance == null) throw new System.Exception("Cannot find valid tile to deploy");
        Tile deployTile = entrance.GetComponent<Tile>();

        Vector2Int offset = DirectionConverter.DirToVect(DirectionConverter.GetOppositeDirection(fromDirection));
        MovePlayerTo(player, deployTile.transform.position + new Vector3(offset.x, playerSpawnYPosition, offset.y));

        NextTurnButton.instance.EnterState(NextTurnButton.State.EXPLORATION);
    }
}
