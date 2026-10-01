using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Room))]
public class CombatPlayerDeploy : PlayerDeploy
{
    private Transform player;

    /// <summary>
    /// The tiles of the layout's deploy cells, the first one by default
    /// </summary>
    private List<Tile> DeployTiles => room.Layout.deployCells.ConvertAll(cell =>
        room.TileAt(cell) ?? throw new System.Exception(room.name + " has no tile on its deploy cell " + cell));

    /// <summary>
    /// Deploys the player in the room.
    /// If enemies are present, gives the choice between all the deploy tiles.
    /// Else, deploys the protagonist on the transitionTile corresponding to the room he comes from.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="fromDirection"></param>
    /// <returns></returns>
	public override IEnumerator DeployPlayer(Transform player, Direction fromDirection) {
        if (GetComponentInChildren<EnemyStats>() == null) {
            // A room without exits (the boss room, entered empty in the room gallery) has no entrance to deploy beside
            if (Entrance(fromDirection) != null || DeployTiles.Count == 0) DefaultDeploy(player, fromDirection);
            else MovePlayerToTile(player, DeployTiles[0]);
            yield break;
        }

        this.player = player;
        player.localEulerAngles = new Vector3(0, -90, 0);
        List<Tile> tiles = DeployTiles;
        if (tiles.Count == 0) throw new System.Exception(room.name + " has enemies but no deploy cell");
        foreach (Tile deployTile in tiles)
            deployTile.Selection = Tile.SelectionType.DEPLOY;

        MovePlayerToTile(player, tiles[0]);
        Room.TileClicked += OnDeployTileClick;
        GameEvents.StartDeploy();

        yield return GameScene.UI.Fade.Reveal();
        GameScene.UI.Hud.EnterDeployState(EndDeployPhase);

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// In the deploy phase, clicking a deploy tile moves the character on it
    /// </summary>
    /// <param name="tile"></param>
    private void OnDeployTileClick(Tile tile) {
        if (tile == null || !DeployTiles.Contains(tile)) return;
        MovePlayerToTile(player, tile);
    }

    /// <summary>
    /// Ends the deploy phase and stops listening to the clicks
    /// </summary>
    public void EndDeployPhase() {
        isDone = true;
        Room.TileClicked -= OnDeployTileClick;
    }
}
