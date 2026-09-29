using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerDeploy))]
public class Room : MonoBehaviour
{
    public RoomType type;

    /// <summary>
    /// Fired when the pointer moves to another tile of the current room, with null when it leaves the tiles
    /// </summary>
    public static event System.Action<Tile> TileHovered;

    /// <summary>
    /// Fired when a selectable tile of the current room is clicked
    /// </summary>
    public static event System.Action<Tile> TileClicked;

    /// <summary>
    /// Fired when a tile of the current room that is not selectable is clicked
    /// </summary>
    public static event System.Action<Tile> UnselectableTileClicked;

    /// <summary>
    /// Fired when the entity on the hovered tile changes (its tile, its model or its timeline item is hovered), with null
    /// when none, and when the same entity goes from being pointed at from the UI to being hovered on the board or back
    /// </summary>
    public static event System.Action<TacticsMove> EntityHovered;

    /// <summary>
    /// The tile of the current room under the pointer, null if none
    /// </summary>
    public static Tile HoveredTile { get; private set; }

    /// <summary>
    /// The entity on <see cref="HoveredTile"/>, null if none; with <see cref="EditionProfile.infoOnModelHover"/>, the entity
    /// whose model is under the pointer first
    /// </summary>
    public static TacticsMove HoveredEntity { get; private set; }

    //The entity pointed at from the UI (the timeline): the board acts as if the pointer were on its tile
    private static TacticsMove pointedEntity;
    //Whether HoveredEntity was pointed at from the UI
    private static bool hoveredFromUI;

    [SerializeField] private List<GameObject> lTilePossible;

    [SerializeField, Tooltip("The room's terrain, deploy cells and enemy layouts, painted and baked in its inspector")]
    [Sirenix.OdinInspector.InlineEditor(Expanded = true)]
    private RoomLayout layout;

    public RoomLayout Layout => layout;

    private readonly Dictionary<Vector2Int, Tile> tilesByCell = new Dictionary<Vector2Int, Tile>();


    private readonly List<TransitionTile> exits = new List<TransitionTile>();

    public IReadOnlyList<TransitionTile> Exits => exits;

    /// <summary>
    /// The room's tiles, exits included
    /// </summary>
    public Tile[] Tiles { get; private set; }

    /// <summary>
    /// The box around the centers of the tiles
    /// </summary>
    public Bounds TileBounds { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        TileHovered = null;
        TileClicked = null;
        UnselectableTileClicked = null;
        EntityHovered = null;
        HoveredTile = null;
        HoveredEntity = null;
        pointedEntity = null;
        hoveredFromUI = false;
    }

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
            TurnSystem.Instance.RegisterEnemy(enemy);
        }
    }

    /// <summary>
    /// Where an entity standing on the tile has its feet
    /// </summary>
    public static Vector3 TileTop(Tile tile) => tile.transform.position + Vector3.up * tile.GetComponent<Collider>().bounds.extents.y;

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
        GameInput.Controls.Gameplay.Select.performed += OnSelect;
        GameEvents.CombatStarted += LockExits;
        GameEvents.CombatEnded += SpawnReward;
        GameEvents.ExplorationStarted += UnlockExits;
    }

    private void OnDisable() {
        GameInput.Controls.Gameplay.Select.performed -= OnSelect;
        GameEvents.CombatStarted -= LockExits;
        GameEvents.CombatEnded -= SpawnReward;
        GameEvents.ExplorationStarted -= UnlockExits;
        //The room is disabled when the player leaves it
        HoveredTile = null;
        HoveredEntity = null;
    }

    /// <summary>
    /// Makes the board act as if the pointer were on the entity's tile, until <see cref="StopPointingAt"/>: hovering and
    /// clicking an entity in the UI works as on its tile
    /// </summary>
    public static void PointAt(TacticsMove entity) => pointedEntity = entity;

    /// <summary>
    /// The board is pointed at an entity from the UI rather than by the pointer: the UI shows the entity's info itself
    /// </summary>
    public static bool IsPointedFromUI => pointedEntity != null;

    public static void StopPointingAt(TacticsMove entity) {
        if (pointedEntity == entity) pointedEntity = null;
    }

	/// <summary>
    /// Updates the hovered tile and entity. Done every frame: the pointer or the entities move, and the hover highlight
    /// must be restored after the tiles are reset
    /// </summary>
	void Update()
    {
        Tile hovered = pointedEntity == null ? Tile.FindHoveredTile()
            : GameScene.IsGameplayBlocked ? null : pointedEntity.CurrentTile;
        if (hovered != HoveredTile) {
            //Before the modes paint the tiles for the new one, which may use the old one
            if (HoveredTile != null) HoveredTile.IsTarget = false;
            HoveredTile = hovered;
            TileHovered?.Invoke(hovered);
        }
        //After the modes, which reset the target tiles when the hovered one changes: every paint keeps a target tile's
        if (hovered != null && hovered.isWalkable && !hovered.IsTarget) hovered.IsTarget = true;

        TacticsMove entity = hovered != null ? hovered.GetEntity() : null;
        //The original showed an entity's info on its model, whatever tile is picked behind it
        if (pointedEntity == null && Edition.Profile.infoOnModelHover) {
            TacticsMove model = Tile.FindHoveredModel();
            if (model != null) entity = model;
        }
        bool fromUI = IsPointedFromUI;
        if (entity == HoveredEntity && fromUI == hoveredFromUI) return;
        HoveredEntity = entity;
        hoveredFromUI = fromUI;
        EntityHovered?.Invoke(entity);
    }

    private void OnSelect(InputAction.CallbackContext context) {
        if (HoveredTile == null) return;
        if (HoveredTile.Selection != Tile.SelectionType.NONE) TileClicked?.Invoke(HoveredTile);
        else UnselectableTileClicked?.Invoke(HoveredTile);
    }

    private void LockExits() => SetExitsOpen(false);

    private void UnlockExits() => SetExitsOpen(true);

    private void SetExitsOpen(bool open) {
        foreach (TransitionTile exit in exits)
            exit.SetOpen(open);
	}

    private void ReloadTilesWithRandomPrefab() {
        //The exits, tagged MapChangerTile, keep their model
        foreach (Tile tile in Tiles) {
            if (!tile.CompareTag("Tile")) continue;
            tile.GetComponent<MeshFilter>().sharedMesh = lTilePossible[Random.Range(0, lTilePossible.Count)].GetComponent<MeshFilter>().sharedMesh;
            tile.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            tile.FindNeighbors();
        }
    }

    /// <summary>
    /// On combat end, spawns a reward
    /// </summary>
    private void SpawnReward() {
        TreasureSpawnPoint rewardSpawnPoint = GetComponentInChildren<TreasureSpawnPoint>();
        if (rewardSpawnPoint != null)
            rewardSpawnPoint.Spawn();
    }

}
