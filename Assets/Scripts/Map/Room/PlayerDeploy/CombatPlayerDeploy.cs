using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Room))]
public class CombatPlayerDeploy : PlayerDeploy
{
    public List<Tile> deployTiles; //Editable in inspector
    private float playerSpawnYPosition = 0.5f;
    private Transform player;

    [HideInInspector] bool isDone = false;

    private Room room;

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
	public override IEnumerator DeployPlayer(Transform player, Direction fromDirection) {
        this.player = player;

        if (GetComponentInChildren<EnemyStats>() != null) { //The room contains enemies
            player.localEulerAngles = new Vector3(0, -90, 0);
            foreach (Tile deployTile in deployTiles) {
                deployTile.Selection = Tile.SelectionType.DEPLOY;
            }

            player.position = deployTiles[0].transform.position + Vector3.up * playerSpawnYPosition;

            room.tileClicked.AddListener(OnDeployTileClick);

            yield return FindObjectOfType<UIFade>().FadeOut();
            NextTurnButton.instance.EnterState(NextTurnButton.State.DEPLOY);

            yield return new WaitUntil(() => isDone);
        }        
        else {
            DefaultDeploy(player, fromDirection);
		}
    }

    /// <summary>
    /// In the deploy phase, clicking a tile moves the character and
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

        room.tileClicked.RemoveListener(OnDeployTileClick);
    }
}
