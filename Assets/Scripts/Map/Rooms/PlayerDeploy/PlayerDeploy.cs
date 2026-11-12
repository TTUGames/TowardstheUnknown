using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Places the player in its room: with enemies, the player chooses among the layout's deploy cells; without, beside the
/// exit it comes from, or on the first deploy cell when the room has no exit on that side (the start of a run, the boss
/// room entered empty in the room gallery)
/// </summary>
[RequireComponent(typeof(Room))]
public class PlayerDeploy : MonoBehaviour
{
    private const float PlayerSpawnHeight = 0.5f;

    private bool isDone;
    private Room room;
    // Set while the player chooses its tile
    private Transform player;

    private void Awake() {
        room = GetComponent<Room>();
    }

    /// <summary>
    /// Whether the player chooses a deploy tile in the current room, for the visuals turned back on during the phase
    /// (the edition switched), whose <see cref="GameEvents.DeployStarted"/> has passed
    /// </summary>
    public static bool InDeployPhase => GameScene.Map != null && GameScene.Map.CurrentRoom != null
        && GameScene.Map.CurrentRoom.TryGetComponent(out PlayerDeploy deploy) && deploy.player != null && !deploy.isDone;

    // The tiles of the layout's deploy cells, the first one by default
    private List<Tile> DeployTiles => room.Layout == null ? new List<Tile>() : room.Layout.deployCells.ConvertAll(cell =>
        room.TileAt(cell) ?? throw new System.Exception(room.name + " has no tile on its deploy cell " + cell));

    public IEnumerator DeployPlayer(Transform player, Direction fromDirection) {
        List<Tile> tiles = DeployTiles;
        if (GetComponentInChildren<EnemyStats>() == null) {
            TransitionTile entrance = null;
            foreach (TransitionTile exit in room.Exits)
                if (exit.direction == fromDirection) entrance = exit;
            if (entrance != null) {
                Vector2Int offset = DirectionConverter.DirToVect(DirectionConverter.GetOppositeDirection(fromDirection));
                MovePlayerTo(player, entrance.transform.position + new Vector3(offset.x, PlayerSpawnHeight, offset.y));
            }
            else if (tiles.Count > 0) MovePlayerTo(player, tiles[0]);
            else throw new System.Exception(room.name + " has neither an exit on the side the player comes from nor a deploy cell");
            // The room's objects start before the combat check
            yield return null;
            yield break;
        }

        if (tiles.Count == 0) throw new System.Exception(room.name + " has enemies but no deploy cell");
        this.player = player;
        player.localEulerAngles = new Vector3(0, -90, 0);
        foreach (Tile deployTile in tiles)
            deployTile.Selection = Tile.SelectionType.DEPLOY;

        MovePlayerTo(player, tiles[0]);
        FaceEnemies();
        BoardPointer.TileClicked += Choose;
        GameEvents.StartDeploy();

        yield return GameScene.UI.Fade.Reveal();
        GameEvents.ShowDeployChoice(this);

        yield return new WaitUntil(() => isDone);
    }

    private static void MovePlayerTo(Transform player, Tile tile) =>
        MovePlayerTo(player, tile.transform.position + Vector3.up * PlayerSpawnHeight);

    private static void MovePlayerTo(Transform player, Vector3 position) {
        player.position = position;
        player.GetComponent<TacticsMove>().SetCurrentTileFromRaycast();
    }

    /// <summary>
    /// In the deploy phase, a deploy tile clicked (or replayed, <see cref="PlayerCommands"/>) moves the character on it
    /// </summary>
    public void Choose(Tile tile) {
        if (tile == null || !DeployTiles.Contains(tile)) return;
        Vector3 from = player.position;
        MovePlayerTo(player, tile);
        FaceEnemies();
        if ((player.position - from).sqrMagnitude > 0.01f) GameEvents.Redeploy(player.GetComponent<EntityStats>(), from);
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
        Room.Face(player, middle / enemies.Length);
    }

    /// <summary>
    /// Ends the deploy phase and stops listening to the clicks
    /// </summary>
    public void EndDeployPhase() {
        if (!isDone && player != null && player.TryGetComponent(out TacticsMove move))
            CommandLog.Record(new GameCommand { type = CommandType.Deploy, cell = room.CellOf(move.CurrentTile) });
        isDone = true;
        BoardPointer.TileClicked -= Choose;
    }

    // A room left during the deploy phase must not keep listening
    private void OnDisable() => BoardPointer.TileClicked -= Choose;
}
