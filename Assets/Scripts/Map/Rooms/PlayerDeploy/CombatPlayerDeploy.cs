using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Room))]
public class CombatPlayerDeploy : PlayerDeploy
{
    private Transform player;

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
        FaceEnemies();
        BoardPointer.TileClicked += OnDeployTileClick;
        GameEvents.StartDeploy();

        yield return GameScene.UI.Fade.Reveal();
        GameEvents.ShowDeployChoice(this);

        yield return new WaitUntil(() => isDone);
    }

    /// <summary>
    /// In the deploy phase, clicking a deploy tile moves the character on it
    /// </summary>
    /// <param name="tile"></param>
    private void OnDeployTileClick(Tile tile) {
        if (tile == null || !DeployTiles.Contains(tile)) return;
        MovePlayerToTile(player, tile);
        FaceEnemies();
    }

    /// <summary>
    /// Turns the player towards the middle of the room's enemies (EditionProfile.deployFacing); otherwise it keeps facing west, as the original
    /// </summary>
    private void FaceEnemies() {
        if (!Edition.Profile.deployFacing) return;
        EnemyStats[] enemies = GetComponentsInChildren<EnemyStats>();
        if (enemies.Length == 0) return;
        Vector3 middle = Vector3.zero;
        foreach (EnemyStats enemy in enemies) middle += enemy.transform.position;
        Vector3 towards = middle / enemies.Length - player.position;
        towards.y = 0;
        if (towards.sqrMagnitude > 1e-4f) player.rotation = Quaternion.LookRotation(towards);
    }

    /// <summary>
    /// Ends the deploy phase and stops listening to the clicks
    /// </summary>
    public void EndDeployPhase() {
        isDone = true;
        BoardPointer.TileClicked -= OnDeployTileClick;
    }
}
