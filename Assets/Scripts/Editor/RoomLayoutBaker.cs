using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Reads a room's tiles into its <see cref="RoomLayout"/> (Import) and writes the layout's terrain back into the tiles (Bake).
/// The bake only touches the cells whose terrain changed: the others keep their tile as it is (model, rotation, overrides),
/// so that an import followed by a bake changes nothing. The decor (rocks around a hole, a wall's model) is not baked
/// </summary>
public static class RoomLayoutBaker
{
    private const string TilePrefabPath = "Assets/Prefabs/LevelDesign/Tile.prefab";
    /// <summary>The room's child holding the tiles, grouped by terrain (see <see cref="GroupOf"/>)</summary>
    public const string BoardName = "Board";
    private const string LegacyBoardName = "Tilemap";

    /// <summary>
    /// The terrain a tile stands for: an exit, a floor, a wall (taller than the floor, in a wall group, or under a collider
    /// such as a fence) or a hole
    /// </summary>
    public static RoomLayout.Cell Classify(Tile tile, float floorHeight, IReadOnlyList<Collider> blockers) {
        if (tile.GetComponent<TransitionTile>() != null) return RoomLayout.Cell.Exit;
        if (tile.isWalkable) return RoomLayout.Cell.Floor;
        if (tile.transform.parent.name.ToLowerInvariant().Contains("wall")) return RoomLayout.Cell.Wall;
        Bounds bounds = tile.GetComponent<Collider>().bounds;
        if (bounds.size.y > floorHeight * 1.05f) return RoomLayout.Cell.Wall;
        Bounds above = new Bounds(bounds.center + Vector3.up * (bounds.extents.y + 0.5f), new Vector3(0.4f, 0.9f, 0.4f));
        if (blockers.Any(blocker => blocker.bounds.Intersects(above))) return RoomLayout.Cell.Wall;
        return RoomLayout.Cell.Hole;
    }

    /// <summary>
    /// The terrain of each tile of a room, as <see cref="Bake"/> sees it: the floor's height is the first plain floor tile's
    /// </summary>
    public static Dictionary<Tile, RoomLayout.Cell> ClassifyAll(GameObject root) {
        Tile[] tiles = root.GetComponentsInChildren<Tile>(true);
        Tile reference = FloorReference(tiles);
        float floorHeight = reference != null ? reference.GetComponent<Collider>().bounds.size.y : 1f;
        List<Collider> blockers = Blockers(root);
        return tiles.ToDictionary(tile => tile, tile => Classify(tile, floorHeight, blockers));
    }

    /// <summary>
    /// The board's group of a terrain: Floor, Exits, Void (holes) or Walls
    /// </summary>
    public static string GroupOf(RoomLayout.Cell type) => type switch {
        RoomLayout.Cell.Exit => "Exits",
        RoomLayout.Cell.Wall => "Walls",
        RoomLayout.Cell.Hole => "Void",
        _ => "Floor",
    };

    /// <summary>
    /// The room's board (its legacy name, Tilemap, is still found), null if none
    /// </summary>
    public static Transform FindBoard(GameObject root) => root.transform.Find(BoardName) ?? root.transform.Find(LegacyBoardName);

    /// <summary>
    /// The name of the tile of a cell of the layout
    /// </summary>
    public static string TileName(Vector2Int cell) => $"Tile_{cell.x:00}_{cell.y:00}";

    private static Tile FloorReference(IEnumerable<Tile> tiles) =>
        tiles.FirstOrDefault(tile => tile.isWalkable && tile.GetComponent<TransitionTile>() == null);

    private static List<Collider> Blockers(GameObject root) =>
        root.GetComponentsInChildren<Collider>(true).Where(c => c.enabled && !c.isTrigger && c.GetComponent<Tile>() == null).ToList();

    /// <summary>
    /// The prefabs of the rooms using a layout
    /// </summary>
    public static List<string> RoomsUsing(RoomLayout layout) =>
        AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Rooms" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => AssetDatabase.LoadAssetAtPath<GameObject>(path).TryGetComponent(out Room room) && room.Layout == layout)
            .ToList();

    /// <summary>
    /// Runs an edit on a room prefab: in its open Prefab Mode if it is open there (with undo), on its contents otherwise
    /// </summary>
    public static T EditRoom<T>(string path, System.Func<GameObject, T> edit) {
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && stage.assetPath == path) {
            Undo.RegisterFullObjectHierarchyUndo(stage.prefabContentsRoot, "Bake Room Layout");
            T result = edit(stage.prefabContentsRoot);
            EditorSceneManager.MarkSceneDirty(stage.scene);
            return result;
        }
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try {
            T result = edit(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            return result;
        }
        finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    /// <summary>
    /// Reads the terrain and the exits from the room's tiles, the room centered in the grid. The deploy cells and the enemy
    /// layouts are kept, moved along if the room moves in the grid
    /// </summary>
    public static string Import(GameObject root, RoomLayout layout) {
        Room room = root.GetComponent<Room>();
        Tile[] tiles = root.GetComponentsInChildren<Tile>(true);
        if (tiles.Length == 0) return root.name + " has no tile";
        Vector3 Local(Transform t) => room.transform.InverseTransformPoint(t.position);

        List<Vector3> positions = tiles.Select(tile => Local(tile.transform)).ToList();
        float fracX = Mathf.Repeat(MostCommon(positions.Select(p => Mathf.Repeat(p.x, 1f))), 1f);
        float fracZ = Mathf.Repeat(MostCommon(positions.Select(p => Mathf.Repeat(p.z, 1f))), 1f);
        int minX = positions.Min(p => Mathf.RoundToInt(p.x - fracX)), maxX = positions.Max(p => Mathf.RoundToInt(p.x - fracX));
        int minZ = positions.Min(p => Mathf.RoundToInt(p.z - fracZ)), maxZ = positions.Max(p => Mathf.RoundToInt(p.z - fracZ));
        if (maxX - minX >= RoomLayout.Size || maxZ - minZ >= RoomLayout.Size)
            return $"{root.name} is {maxX - minX + 1}x{maxZ - minZ + 1} tiles, over the {RoomLayout.Size}x{RoomLayout.Size} grid";

        Tile[] floors = tiles.Where(tile => tile.isWalkable).DefaultIfEmpty(tiles[0]).ToArray();
        float floorY = MostCommon(floors.Select(tile => Local(tile.transform).y));
        float floorHeight = MostCommon(floors.Select(tile => tile.GetComponent<Collider>().bounds.size.y));
        int marginX = (RoomLayout.Size - (maxX - minX + 1)) / 2, marginZ = (RoomLayout.Size - (maxZ - minZ + 1)) / 2;

        Undo.RecordObject(layout, "Import Room Layout");
        Vector3 oldOrigin = layout.origin;
        layout.origin = new Vector3(minX + fracX - marginX, floorY, minZ + fracZ - marginZ);
        for (int x = 0; x < RoomLayout.Size; x++)
            for (int y = 0; y < RoomLayout.Size; y++) layout.Set(new Vector2Int(x, y), RoomLayout.Cell.Empty);
        layout.exits.Clear();

        List<Collider> blockers = Blockers(root);
        foreach (Tile tile in tiles) {
            Vector2Int cell = layout.LocalToCell(Local(tile.transform));
            RoomLayout.Cell type = Classify(tile, floorHeight, blockers);
            layout.Set(cell, type);
            if (type == RoomLayout.Cell.Exit)
                layout.exits.Add(new RoomLayout.Exit { cell = cell, direction = tile.GetComponent<TransitionTile>().direction });
        }

        //The deploy cells and the enemies stay on their tiles when the room is centered again
        Vector3 shift = oldOrigin - layout.origin;
        Vector2Int cellShift = new Vector2Int(Mathf.RoundToInt(shift.x), Mathf.RoundToInt(shift.z));
        if (cellShift != Vector2Int.zero) {
            layout.deployCells = layout.deployCells.Select(cell => cell + cellShift).ToList();
            foreach (RoomLayout.EnemyPlacement enemy in layout.enemyLayouts.SelectMany(l => l.enemies)) enemy.cell += cellShift;
        }
        EditorUtility.SetDirty(layout);
        return $"{root.name}: {tiles.Length} tiles, {layout.exits.Count} exits" + (cellShift != Vector2Int.zero ? $", deploy cells and enemies moved by {cellShift}" : "");
    }

    /// <summary>
    /// Writes the layout's terrain into the room's tiles: adds, removes and reconfigures the tiles of the changed cells.
    /// A dry run only counts them, touching nothing
    /// </summary>
    public static string Bake(GameObject root, RoomLayout layout, bool dryRun = false) => Bake(root, layout, dryRun, out _);

    /// <inheritdoc cref="Bake(GameObject, RoomLayout, bool)"/>
    public static string Bake(GameObject root, RoomLayout layout, bool dryRun, out int changes) {
        changes = 0;
        Room room = root.GetComponent<Room>();
        Transform board = FindBoard(root);
        if (board == null && !dryRun) {
            board = new GameObject(BoardName).transform;
            board.SetParent(root.transform, false);
        }

        Dictionary<Vector2Int, Tile> existing = new Dictionary<Vector2Int, Tile>();
        foreach (Tile tile in root.GetComponentsInChildren<Tile>(true)) {
            Vector2Int cell = layout.LocalToCell(room.transform.InverseTransformPoint(tile.transform.position));
            if (existing.ContainsKey(cell)) {
                changes = -1;
                return $"{root.name}: two tiles on {cell}, fix them before baking";
            }
            existing[cell] = tile;
        }

        Tile reference = FloorReference(existing.Values);
        Tile tilePrefab = AssetDatabase.LoadAssetAtPath<Tile>(TilePrefabPath);
        float floorHeight = reference != null ? reference.GetComponent<Collider>().bounds.size.y : 1f;
        List<Collider> blockers = Blockers(root);

        int added = 0, removed = 0, changed = 0;
        for (int x = 0; x < RoomLayout.Size; x++) {
            for (int y = 0; y < RoomLayout.Size; y++) {
                Vector2Int cell = new Vector2Int(x, y);
                RoomLayout.Cell type = layout.Get(cell);
                existing.TryGetValue(cell, out Tile tile);

                if (type == RoomLayout.Cell.Empty) {
                    if (tile == null) continue;
                    if (!dryRun) Object.DestroyImmediate(tile.gameObject);
                    removed++;
                    continue;
                }
                if (tile != null && Classify(tile, floorHeight, blockers) == type) {
                    if (type != RoomLayout.Cell.Exit) continue;
                    if (!dryRun) SetExit(tile, layout.ExitDirection(cell));
                    else if (ExitChanges(tile, layout.ExitDirection(cell))) changed++;
                    continue;
                }
                if (dryRun) {
                    if (tile == null) added++;
                    else changed++;
                    continue;
                }
                if (tile == null) {
                    tile = (Tile)PrefabUtility.InstantiatePrefab(tilePrefab, board);
                    tile.name = TileName(cell);
                    tile.transform.position = room.transform.TransformPoint(layout.CellToLocal(cell));
                    if (reference != null) {
                        tile.transform.localScale = reference.transform.localScale;
                        tile.transform.rotation = reference.transform.rotation;
                        tile.GetComponent<MeshFilter>().sharedMesh = reference.GetComponent<MeshFilter>().sharedMesh;
                        tile.GetComponent<MeshRenderer>().sharedMaterial = reference.GetComponent<MeshRenderer>().sharedMaterial;
                    }
                    added++;
                }
                else changed++;
                Configure(tile, type, layout, cell, board, reference);
            }
        }
        changes = added + removed + changed;
        return dryRun
            ? $"{root.name}: {added} tiles to add, {removed} to remove, {changed} to change"
            : $"{root.name}: {added} tiles added, {removed} removed, {changed} changed";
    }

    private static void Configure(Tile tile, RoomLayout.Cell type, RoomLayout layout, Vector2Int cell, Transform board, Tile reference) {
        bool walkable = type == RoomLayout.Cell.Floor || type == RoomLayout.Cell.Exit;
        tile.isWalkable = walkable;
        tile.GetComponent<MeshRenderer>().enabled = walkable;

        Vector3 scale = tile.transform.localScale;
        float floorScale = reference != null ? reference.transform.localScale.y : scale.y;
        scale.y = type == RoomLayout.Cell.Wall ? floorScale * layout.wallHeight : floorScale;
        tile.transform.localScale = scale;

        Transform parent = Group(board, GroupOf(type));
        if (tile.transform.parent != parent) tile.transform.SetParent(parent, true);

        if (type == RoomLayout.Cell.Exit) SetExit(tile, layout.ExitDirection(cell));
        else if (tile.TryGetComponent(out TransitionTile exit)) Object.DestroyImmediate(exit, true);
        EditorUtility.SetDirty(tile);
    }

    private static bool ExitChanges(Tile tile, Direction direction) =>
        !tile.TryGetComponent(out TransitionTile exit) || (direction != Direction.NULL && exit.direction != direction);

    private static void SetExit(Tile tile, Direction direction) {
        if (!tile.TryGetComponent(out TransitionTile exit)) exit = tile.gameObject.AddComponent<TransitionTile>();
        if (direction != Direction.NULL && exit.direction != direction) {
            exit.direction = direction;
            EditorUtility.SetDirty(exit);
        }
    }

    // The board's group of a terrain, created if missing
    private static Transform Group(Transform board, string name) {
        Transform group = board.Find(name);
        if (group != null) return group;
        group = new GameObject(name).transform;
        group.SetParent(board, false);
        return group;
    }

    private static float MostCommon(IEnumerable<float> values) =>
        values.GroupBy(v => Mathf.Round(v * 100f) / 100f).OrderByDescending(g => g.Count()).First().Key;
}

public static class RoomLayoutMenu
{
    private const string LayoutFolder = "Assets/Data/Rooms/Layouts";

    /// <summary>
    /// Gives each room prefab without a layout its own, imported from its tiles
    /// </summary>
    [MenuItem("Tools/Level Design/Create Missing Room Layouts")]
    public static void CreateMissingLayouts() => Debug.Log(CreateMissing());

    public static string CreateMissing() {
        if (!AssetDatabase.IsValidFolder(LayoutFolder)) AssetDatabase.CreateFolder("Assets/Data/Rooms", "Layouts");
        List<string> reports = new List<string>();
        foreach (string path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Rooms" }).Select(AssetDatabase.GUIDToAssetPath)) {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(path).TryGetComponent(out Room room) || room.Layout != null) continue;
            RoomLayout layout = ScriptableObject.CreateInstance<RoomLayout>();
            AssetDatabase.CreateAsset(layout, $"{LayoutFolder}/{room.name}.asset");
            reports.Add(RoomLayoutBaker.EditRoom(path, root => {
                string report = RoomLayoutBaker.Import(root, layout);
                SerializedObject serialized = new SerializedObject(root.GetComponent<Room>());
                serialized.FindProperty("layout").objectReferenceValue = layout;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                return report;
            }));
        }
        AssetDatabase.SaveAssets();
        return reports.Count == 0 ? "Every room has a layout" : string.Join("\n", reports);
    }
}
