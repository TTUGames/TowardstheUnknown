using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Room))]
public class CombatPlayerDeploy : PlayerDeploy
{
    public List<Tile> deployTiles; //Editable in inspector
    private Transform player;

    /// <summary>
    /// Deploys the player in the room.
    /// If enemies are present, gives the choice between all deployTiles.
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
            if (hasEntrance || deployTiles.Count == 0) DefaultDeploy(player, fromDirection);
            else MovePlayerToTile(player, deployTiles[0]);
            yield break;
        }

        this.player = player;
        player.localEulerAngles = new Vector3(0, -90, 0);
        foreach (Tile deployTile in deployTiles)
            deployTile.Selection = Tile.SelectionType.DEPLOY;

        MovePlayerToTile(player, deployTiles[0]);
        Room.TileClicked += OnDeployTileClick;
        GameEvents.StartDeploy();

        yield return GameScene.UI.Fade.FadeOut();
        GameScene.UI.Hud.EnterDeployState(EndDeployPhase);

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// In the deploy phase, clicking a deploy tile moves the character on it
    /// </summary>
    /// <param name="tile"></param>
    private void OnDeployTileClick(Tile tile) {
        if (tile == null || !deployTiles.Contains(tile)) return;
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
