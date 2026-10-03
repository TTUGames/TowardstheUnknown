using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerDeploy))]
public class Room : MonoBehaviour
{
    public RoomType type;
    [Tooltip("Where it lies, for its ambience (AmbienceDirector); the antechamber and the boss room are Drareg's garden")]
    public RoomPlace place;

    [SerializeField] private List<GameObject> lTilePossible;

    [SerializeField, Tooltip("The room's terrain, deploy cells and enemy layouts, painted and baked in its inspector")]
    [Sirenix.OdinInspector.InlineEditor(Expanded = true)]
    private RoomLayout layout;

    public RoomLayout Layout => layout;

    private readonly Dictionary<Vector2Int, Tile> tilesByCell = new Dictionary<Vector2Int, Tile>();


    private readonly List<TransitionTile> exits = new List<TransitionTile>();

    public IReadOnlyList<TransitionTile> Exits => exits;

    // The collectables lying in the room
    private int loot;

    /// <summary>
    /// A relic lies in the room, not picked up yet
    /// </summary>
    public bool HasLoot => loot > 0;

    /// <summary>
    /// Counts a collectable in (1) or out (-1) of the room: its own as it spawns and is picked up
    /// </summary>
    public void CountLoot(int change) {
        loot += change;
        GameEvents.ChangeLoot(this);
    }

    /// <summary>
    /// The room's tiles, exits included
    /// </summary>
    public Tile[] Tiles { get; private set; }

    /// <summary>
    /// The box around the centers of the tiles
    /// </summary>
    public Bounds TileBounds { get; private set; }

    private void Awake() {
        Tiles = GetComponentsInChildren<Tile>();
        if (Tiles.Length > 0) {
            Bounds bounds = new Bounds(Tiles[0].transform.position, Vector3.zero);
            foreach (Tile tile in Tiles) bounds.Encapsulate(tile.transform.position);
            TileBounds = bounds;
        }
        exits.AddRange(GetComponentsInChildren<TransitionTile>());
        if (layout != null)
            foreach (Tile tile in Tiles) tilesByCell[layout.LocalToCell(transform.InverseTransformPoint(tile.transform.position))] = tile;
        ReloadTilesWithRandomPrefab();
    }

    /// <summary>
    /// Removes the exits leading nowhere and adds the VFX shown on the others once they open
    /// </summary>
    public void SetExits(System.Func<Direction, bool> hasExit, GameObject exitVFX) {
        foreach (TransitionTile exit in exits) {
            if (hasExit(exit.direction)) exit.AddVFX(exitVFX);
            else DestroyImmediate(exit);
		}
        exits.RemoveAll(exit => exit == null);
	}

    /// <summary>
    /// The tile on a cell of the room's layout, null if none
    /// </summary>
    public Tile TileAt(Vector2Int cell) => tilesByCell.TryGetValue(cell, out Tile tile) ? tile : null;

    /// <summary>
    /// The number of layouts <see cref="RoomInfo"/> can pick from: the enemy layouts of <see cref="Layout"/>, then the
    /// <c>SpawnLayout</c> components (the treasures)
    /// </summary>
    public int LayoutCount => EnemyLayoutCount + GetComponentsInChildren<SpawnLayout>().Length;

    private int EnemyLayoutCount => layout != null ? layout.enemyLayouts.Count : 0;

    private void SpawnLayoutAt(int index) {
        if (index < EnemyLayoutCount) SpawnEnemies(layout.enemyLayouts[index]);
        else GetComponentsInChildren<SpawnLayout>()[index - EnemyLayoutCount].Spawn();
    }

    private void SpawnEnemies(RoomLayout.EnemyLayout enemyLayout) {
        foreach (RoomLayout.EnemyPlacement placement in enemyLayout.enemies) {
            Tile tile = TileAt(placement.cell);
            if (tile == null) throw new System.Exception(name + " has no tile on " + placement.cell + " for " + placement.prefab);
            EntityTurn enemy = Instantiate(placement.prefab);
            EditionMaterials.Apply(enemy.gameObject);
            enemy.transform.SetParent(transform);
            enemy.transform.position = TileTop(tile) + transform.TransformVector(placement.offset);
            enemy.GetComponent<TacticsMove>().SetCurrentTileFromRaycast();
            if (Edition.Profile.spawnFacing) FaceDeploy(enemy.transform);
            TurnSystem.Instance.RegisterEnemy(enemy);
        }
    }

    /// <summary>
    /// Turns a spawned enemy towards the middle of the deploy tiles, where the player comes in, or of the room without any
    /// </summary>
    private void FaceDeploy(Transform enemy) {
        Vector3 middle = Vector3.zero;
        int count = 0;
        foreach (Vector2Int cell in layout.deployCells) {
            Tile tile = TileAt(cell);
            if (tile == null) continue;
            middle += tile.transform.position;
            count++;
        }
        Vector3 towards = (count > 0 ? middle / count : transform.position) - enemy.position;
        towards.y = 0;
        if (towards.sqrMagnitude > 1e-4f) enemy.rotation = Quaternion.LookRotation(towards);
    }

    /// <summary>
    /// Where an entity standing on the tile has its feet
    /// </summary>
    public static Vector3 TileTop(Tile tile) => tile.transform.position + Vector3.up * tile.Body.bounds.extents.y;

    /// <summary>
    /// Initializes this room.
    /// Registers the player and the enemies in the turn system. A room left then entered again keeps its loot
    /// </summary>
    /// <param name="info">The room's info. If its layout index is -1, spawns neither enemies nor treasure</param>
    public void Init(RoomInfo info) {
        TurnSystem turnSystem = TurnSystem.Instance;
        turnSystem.Clear();
        PlayerTurn player = GameScene.Player;
        turnSystem.RegisterPlayer(player);

        if (info.GetLayoutIndex() != -1) SpawnLayoutAt(info.GetLayoutIndex());

        turnSystem.NotifyTurnOrderChanged();
        //Before they first play, so that the first show of an effect does not freeze the game
        VFXWarmup.Warm(GetComponentsInChildren<EnemyAI>().SelectMany(enemy => enemy.AllPatterns));
        VFXWarmup.Warm(player.Inventory.GetPlayerArtifacts());
        VFXWarmup.Warm(GetComponentsInChildren<EntityFeedback>().Append(player.GetComponent<EntityFeedback>())
            .Select(feedback => feedback.HitVFX));
        GameEvents.EnterRoom(this, !info.IsAlreadyVisited());
    }

    private void OnEnable() {
        GameEvents.CombatStarted += LockExits;
        GameEvents.CombatEnded += SpawnReward;
        GameEvents.ExplorationStarted += UnlockExits;
    }

    private void OnDisable() {
        GameEvents.CombatStarted -= LockExits;
        GameEvents.CombatEnded -= SpawnReward;
        GameEvents.ExplorationStarted -= UnlockExits;
    }

    private void LockExits() => SetExitsOpen(false);

    private void UnlockExits() => SetExitsOpen(true);

    private void SetExitsOpen(bool open) {
        foreach (TransitionTile exit in exits)
            exit.SetOpen(open);
	}

    private void ReloadTilesWithRandomPrefab() {
        //Every tile, the exits included, takes a random model and turn
        foreach (Tile tile in Tiles) {
            tile.GetComponent<MeshFilter>().sharedMesh = lTilePossible[Random.Range(0, lTilePossible.Count)].GetComponent<MeshFilter>().sharedMesh;
            tile.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            tile.FindNeighbors();
        }
    }

    /// <summary>
    /// On combat end, spawns a reward after the victory's beat (<see cref="EditionProfile.victoryBeat"/>), which holds the player
    /// meanwhile: the queue is busy
    /// </summary>
    private void SpawnReward() {
        TreasureSpawnPoint rewardSpawnPoint = GetComponentInChildren<TreasureSpawnPoint>();
        EditionProfile profile = Edition.Profile;
        if (profile.victoryBeat <= 0) {
            if (rewardSpawnPoint != null) rewardSpawnPoint.Spawn(profile.rewardPopIn);
            return;
        }
        ActionManager.AddToBottom(new WaitAction(profile.victoryBeat));
        if (rewardSpawnPoint != null)
            ActionManager.AddToBottom(() => rewardSpawnPoint.Spawn(Edition.Profile.rewardPopIn));
    }

}
