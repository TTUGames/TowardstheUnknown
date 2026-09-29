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
    private List<Tile> DeployTiles => room.Layout.deployCells.ConvertAll(room.TileAt);

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
            bool hasEntrance = false;
            foreach (TransitionTile exit in room.Exits)
                if (exit.direction == fromDirection) hasEntrance = true;
            if (hasEntrance || DeployTiles.Count == 0) DefaultDeploy(player, fromDirection);
            else MovePlayerToTile(player, DeployTiles[0]);
            yield break;
        }

        this.player = player;
        player.localEulerAngles = new Vector3(0, -90, 0);
        List<Tile> tiles = DeployTiles;
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
    /// Cleans the events and selected tiles
    /// </summary>
    public void EndDeployPhase() {
        isDone = true;
        Room.TileClicked -= OnDeployTileClick;
    }
}
