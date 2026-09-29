using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Paints a <see cref="RoomLayout"/>: the terrain, the exits, the deploy cells and the enemies of each enemy layout, on its
/// grid seen from above (north up). Left click or drag paints with the brush, right click erases. Bake writes the terrain
/// into the tiles of the rooms using the layout; Import reads it back from them
/// </summary>
[CustomEditor(typeof(RoomLayout))]
public class RoomLayoutEditor : OdinEditor
{
    private enum Brush { Floor, Hole, Wall, Exit, Erase, Deploy, Enemy }

    private const float CellSize = 26f;
    private const float Spacing = 2f;

    private static readonly Dictionary<RoomLayout.Cell, Color> CellColors = new Dictionary<RoomLayout.Cell, Color> {
        { RoomLayout.Cell.Empty, new Color(0.13f, 0.13f, 0.13f) },
        { RoomLayout.Cell.Floor, new Color(0.42f, 0.47f, 0.55f) },
        { RoomLayout.Cell.Hole, new Color(0.05f, 0.05f, 0.09f) },
        { RoomLayout.Cell.Wall, new Color(0.55f, 0.36f, 0.22f) },
        { RoomLayout.Cell.Exit, new Color(0.30f, 0.62f, 0.38f) },
    };
    private static readonly Color HoleMarkColor = new Color(0.55f, 0.6f, 0.75f);
    private static readonly Color HoverColor = new Color(1f, 1f, 1f, 0.2f);
    private static readonly Color DeployColor = new Color(0.35f, 0.6f, 1f);

    private static Brush brush = Brush.Floor;
    private static int enemyLayoutIndex;
    private static int enemyPrefabIndex;
    private static List<EntityTurn> enemyPrefabs;

    private bool isPainting;
    private GUIStyle cellLabel;
    private GUIStyle holeLabel;
    private string report;

    private RoomLayout Layout => (RoomLayout)target;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        cellLabel ??= new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        holeLabel ??= new GUIStyle(cellLabel) { normal = { textColor = HoleMarkColor } };

        EditorGUILayout.Space();
        DrawRoomButtons();
        EditorGUILayout.Space();
        DrawBrushes();
        if (brush == Brush.Enemy) DrawEnemyLayouts();
        DrawLegend();
        DrawGrid();
        DrawWarnings();
    }

    private void DrawRoomButtons() {
        List<string> rooms = RoomLayoutBaker.RoomsUsing(Layout);
        SirenixEditorGUI.BeginBox();
        EditorGUILayout.LabelField(rooms.Count == 0 ? "No room uses this layout (set it on a Room)"
            : "Rooms: " + string.Join(", ", rooms.Select(System.IO.Path.GetFileNameWithoutExtension)), EditorStyles.wordWrappedMiniLabel);
        using (new EditorGUI.DisabledScope(rooms.Count == 0)) {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Bake terrain into the room", "Adds, removes and reconfigures the tiles of the cells whose terrain changed; the decor is left as it is")))
                report = string.Join("\n", rooms.Select(path => RoomLayoutBaker.EditRoom(path, root => RoomLayoutBaker.Bake(root, Layout))));
            if (GUILayout.Button(new GUIContent("Import from the room", "Reads the terrain and the exits back from the room's tiles (after editing them by hand); the deploy cells and the enemies are kept"))
                && EditorUtility.DisplayDialog("Import the room", "Replace this layout's terrain and exits with the room's tiles?", "Import", "Cancel"))
                report = RoomLayoutBaker.EditRoom(rooms[0], root => RoomLayoutBaker.Import(root, Layout));
            EditorGUILayout.EndHorizontal();
        }
        if (!string.IsNullOrEmpty(report)) EditorGUILayout.HelpBox(report, MessageType.Info);
        SirenixEditorGUI.EndBox();
    }

    private void DrawBrushes() {
        EditorGUILayout.BeginHorizontal();
        foreach (Brush b in System.Enum.GetValues(typeof(Brush))) {
            GUIStyle style = b == Brush.Floor ? EditorStyles.miniButtonLeft : b == Brush.Enemy ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid;
            if (GUILayout.Toggle(brush == b, b.ToString(), style) && brush != b) brush = b;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(brush switch {
            Brush.Exit => "Exit: its direction follows the side of the room it is on",
            Brush.Deploy => "Deploy: toggles a cell the player can start the fight on; the first one is the default",
            Brush.Enemy => "Enemy: places the selected enemy in the selected layout; right click removes",
            _ => "Left click or drag paints, right click erases",
        }, EditorStyles.centeredGreyMiniLabel);
    }

    private void DrawEnemyLayouts() {
        List<RoomLayout.EnemyLayout> layouts = Layout.enemyLayouts;
        SirenixEditorGUI.BeginBox();
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < layouts.Count; i++) {
            if (GUILayout.Toggle(enemyLayoutIndex == i, $"{layouts[i].name} · ★{layouts[i].difficulty}", EditorStyles.miniButton))
                enemyLayoutIndex = i;
        }
        if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(22))) {
            Record("Add Enemy Layout");
            layouts.Add(new RoomLayout.EnemyLayout { name = "Layout " + (layouts.Count + 1) });
            enemyLayoutIndex = layouts.Count - 1;
        }
        EditorGUILayout.EndHorizontal();

        enemyLayoutIndex = Mathf.Clamp(enemyLayoutIndex, 0, Mathf.Max(0, layouts.Count - 1));
        if (layouts.Count > 0) {
            RoomLayout.EnemyLayout layout = layouts[enemyLayoutIndex];
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField(layout.name);
            int difficulty = EditorGUILayout.IntField("Difficulty", layout.difficulty);
            if (EditorGUI.EndChangeCheck()) {
                Record("Edit Enemy Layout");
                layout.name = newName;
                layout.difficulty = difficulty;
            }
            if (GUILayout.Button("Duplicate", EditorStyles.miniButton, GUILayout.Width(64))) {
                Record("Duplicate Enemy Layout");
                layouts.Insert(enemyLayoutIndex + 1, new RoomLayout.EnemyLayout {
                    name = layout.name + " copy", difficulty = layout.difficulty,
                    enemies = layout.enemies.Select(e => new RoomLayout.EnemyPlacement { prefab = e.prefab, cell = e.cell, offset = e.offset }).ToList(),
                });
                enemyLayoutIndex++;
            }
            if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(50))
                && EditorUtility.DisplayDialog("Delete the layout", $"Delete {layout.name}?", "Delete", "Cancel")) {
                Record("Delete Enemy Layout");
                layouts.RemoveAt(enemyLayoutIndex);
            }
            EditorGUILayout.EndHorizontal();
        }

        enemyPrefabs ??= FindEnemyPrefabs();
        enemyPrefabIndex = EditorGUILayout.Popup("Enemy", enemyPrefabIndex, enemyPrefabs.Select(p => p.name).ToArray());
        SirenixEditorGUI.EndBox();
    }

    // The colors and marks of the cells
    private void DrawLegend() {
        (RoomLayout.Cell cell, string mark, string text)[] entries = {
            (RoomLayout.Cell.Floor, "", "Floor"), (RoomLayout.Cell.Hole, "○", "Hole: blocks the walk"),
            (RoomLayout.Cell.Wall, "■", "Wall: blocks the walk and the sight"), (RoomLayout.Cell.Exit, "▲", "Exit"), (RoomLayout.Cell.Empty, "", "Empty"),
        };
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        foreach ((RoomLayout.Cell cell, string mark, string text) in entries) {
            Rect swatch = GUILayoutUtility.GetRect(14, 14, GUILayout.Width(14), GUILayout.Height(14));
            EditorGUI.DrawRect(swatch, CellColors[cell]);
            if (mark != "") GUI.Label(swatch, mark, cell == RoomLayout.Cell.Hole ? holeLabel : cellLabel);
            GUILayout.Label(text, EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
            GUILayout.Space(6);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGrid() {
        int size = RoomLayout.Size;
        float side = size * CellSize;
        Rect grid = GUILayoutUtility.GetRect(side, side, GUILayout.ExpandWidth(false));
        grid.x = Mathf.Max(grid.x, (EditorGUIUtility.currentViewWidth - side) / 2f);
        Rect CellRect(Vector2Int c) => new Rect(grid.x + c.x * CellSize, grid.y + (size - 1 - c.y) * CellSize, CellSize - Spacing, CellSize - Spacing);

        RoomLayout.EnemyLayout enemies = brush == Brush.Enemy && Layout.enemyLayouts.Count > 0 ? Layout.enemyLayouts[enemyLayoutIndex] : null;
        Event current = Event.current;
        Vector2Int? hovered = null;

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                Vector2Int cell = new Vector2Int(x, y);
                Rect rect = CellRect(cell);
                RoomLayout.Cell type = Layout.Get(cell);
                EditorGUI.DrawRect(rect, CellColors[type]);

                if (type == RoomLayout.Cell.Wall) GUI.Label(rect, "■", cellLabel);
                if (type == RoomLayout.Cell.Hole) GUI.Label(rect, "○", holeLabel);
                if (type == RoomLayout.Cell.Exit) GUI.Label(rect, Arrow(Layout.ExitDirection(cell)), cellLabel);
                int deploy = Layout.deployCells.IndexOf(cell);
                if (deploy >= 0) {
                    SirenixEditorGUI.DrawBorders(rect, 2, DeployColor);
                    GUI.Label(rect, "D" + (deploy + 1), cellLabel);
                }
                RoomLayout.EnemyPlacement enemy = enemies?.enemies.FirstOrDefault(e => e.cell == cell);
                if (enemy != null) {
                    Rect dot = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
                    Sprite icon = Icon(enemy.prefab);
                    if (icon != null) DrawSprite(dot, icon);
                    else {
                        EditorGUI.DrawRect(dot, EnemyColor(enemy.prefab));
                        GUI.Label(rect, Initials(enemy.prefab), cellLabel);
                    }
                    if (enemy.offset != Vector3.zero) GUI.Label(new Rect(rect.xMax - 10, rect.y - 3, 12, 12), "*", cellLabel);
                }

                if (!rect.Contains(current.mousePosition)) continue;
                hovered = cell;
                EditorGUI.DrawRect(rect, HoverColor);
                if ((current.type == EventType.MouseDown || current.type == EventType.MouseDrag && isPainting) && (current.button == 0 || current.button == 1)) {
                    bool erase = current.button == 1;
                    if (current.type == EventType.MouseDown) isPainting = brush != Brush.Deploy && brush != Brush.Enemy;
                    Paint(cell, erase, enemies);
                    current.Use();
                }
            }
        }
        if (current.type == EventType.MouseUp) isPainting = false;
        if (grid.Contains(current.mousePosition)) GUIHelper.RequestRepaint();

        EditorGUILayout.LabelField(hovered is Vector2Int h ? Describe(h, enemies) : " ", EditorStyles.centeredGreyMiniLabel);
    }

    private void Paint(Vector2Int cell, bool erase, RoomLayout.EnemyLayout enemies) {
        switch (brush) {
            case Brush.Deploy: {
                Record("Paint Deploy");
                if (!Layout.deployCells.Remove(cell) && !erase) Layout.deployCells.Add(cell);
                break;
            }
            case Brush.Enemy: {
                if (enemies == null) return;
                Record("Paint Enemy");
                RoomLayout.EnemyPlacement placed = enemies.enemies.FirstOrDefault(e => e.cell == cell);
                if (placed != null) enemies.enemies.Remove(placed);
                if (!erase && enemyPrefabs.Count > 0 && (placed == null || placed.prefab != enemyPrefabs[enemyPrefabIndex]))
                    enemies.enemies.Add(new RoomLayout.EnemyPlacement { prefab = enemyPrefabs[enemyPrefabIndex], cell = cell });
                break;
            }
            default: {
                RoomLayout.Cell type = erase ? RoomLayout.Cell.Empty : brush switch {
                    Brush.Floor => RoomLayout.Cell.Floor,
                    Brush.Hole => RoomLayout.Cell.Hole,
                    Brush.Wall => RoomLayout.Cell.Wall,
                    Brush.Exit => RoomLayout.Cell.Exit,
                    _ => RoomLayout.Cell.Empty,
                };
                if (Layout.Get(cell) == type && (type != RoomLayout.Cell.Exit)) return;
                Record("Paint Terrain");
                Layout.exits.RemoveAll(exit => exit.cell == cell);
                Layout.Set(cell, type);
                if (type == RoomLayout.Cell.Exit) Layout.exits.Add(new RoomLayout.Exit { cell = cell, direction = SideOf(cell) });
                break;
            }
        }
        EditorUtility.SetDirty(Layout);
    }

    private void DrawWarnings() {
        List<string> warnings = new List<string>();
        bool Walkable(Vector2Int c) => Layout.Get(c) == RoomLayout.Cell.Floor || Layout.Get(c) == RoomLayout.Cell.Exit;
        foreach (Vector2Int cell in Layout.deployCells.Where(c => !Walkable(c))) warnings.Add($"Deploy cell {cell} is not walkable");
        foreach (RoomLayout.EnemyLayout layout in Layout.enemyLayouts) {
            foreach (RoomLayout.EnemyPlacement enemy in layout.enemies) {
                if (enemy.prefab == null) warnings.Add($"{layout.name}: an enemy on {enemy.cell} has no prefab");
                if (!Walkable(enemy.cell)) warnings.Add($"{layout.name}: {Name(enemy.prefab)} on {enemy.cell} is not on a walkable cell");
                if (Layout.deployCells.Contains(enemy.cell)) warnings.Add($"{layout.name}: {Name(enemy.prefab)} on {enemy.cell} is on a deploy cell");
            }
            if (layout.enemies.Count == 0) warnings.Add($"{layout.name} has no enemy");
        }
        foreach (Direction direction in new[] { Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST })
            if (Layout.exits.Count(e => e.direction == direction) > 1) warnings.Add($"More than one exit to the {direction}");
        foreach (string warning in warnings) EditorGUILayout.HelpBox(warning, MessageType.Warning);
    }

    private string Describe(Vector2Int cell, RoomLayout.EnemyLayout enemies) {
        string text = $"{cell.x},{cell.y}  {Layout.Get(cell)}";
        if (Layout.Get(cell) == RoomLayout.Cell.Exit) text += " " + Layout.ExitDirection(cell);
        int deploy = Layout.deployCells.IndexOf(cell);
        if (deploy >= 0) text += $"  deploy {deploy + 1}";
        RoomLayout.EnemyPlacement enemy = enemies?.enemies.FirstOrDefault(e => e.cell == cell);
        if (enemy != null) text += "  " + Name(enemy.prefab) + (enemy.offset != Vector3.zero ? $" offset {enemy.offset}" : "");
        return text;
    }

    // The side of the room nearest the cell, among the cells holding something
    private Direction SideOf(Vector2Int cell) {
        List<Vector2Int> used = new List<Vector2Int>();
        for (int x = 0; x < RoomLayout.Size; x++)
            for (int y = 0; y < RoomLayout.Size; y++)
                if (Layout.Get(new Vector2Int(x, y)) != RoomLayout.Cell.Empty) used.Add(new Vector2Int(x, y));
        int minX = used.Min(c => c.x), maxX = used.Max(c => c.x), minY = used.Min(c => c.y), maxY = used.Max(c => c.y);
        (int distance, Direction direction)[] sides = {
            (maxY - cell.y, Direction.NORTH), (cell.y - minY, Direction.SOUTH), (maxX - cell.x, Direction.EAST), (cell.x - minX, Direction.WEST),
        };
        return sides.OrderBy(s => s.distance).First().direction;
    }

    private void Record(string name) => Undo.RecordObject(Layout, name);

    private static string Arrow(Direction direction) => direction switch {
        Direction.NORTH => "▲", Direction.SOUTH => "▼", Direction.EAST => "▶", Direction.WEST => "◀", _ => "?",
    };

    private static string Name(EntityTurn prefab) => prefab != null ? prefab.name : "(none)";

    // The enemy's icon in the turn timeline
    private static Sprite Icon(EntityTurn prefab) =>
        prefab != null && prefab.TryGetComponent(out EntityStats stats) && stats.Data != null ? stats.Data.timelineIcon : null;

    private static void DrawSprite(Rect rect, Sprite sprite) {
        Rect uv = sprite.textureRect;
        Texture2D texture = sprite.texture;
        float aspect = uv.width / uv.height;
        Rect fitted = aspect >= 1 ? new Rect(rect.x, rect.center.y - rect.height / aspect / 2, rect.width, rect.height / aspect)
            : new Rect(rect.center.x - rect.width * aspect / 2, rect.y, rect.width * aspect, rect.height);
        GUI.DrawTextureWithTexCoords(fitted, texture, new Rect(uv.x / texture.width, uv.y / texture.height, uv.width / texture.width, uv.height / texture.height));
    }

    // Kameiko: Ka, GreatKameiko: GK
    private static string Initials(EntityTurn prefab) {
        if (prefab == null) return "?";
        string capitals = new string(prefab.name.Where(char.IsUpper).ToArray());
        return capitals.Length >= 2 ? capitals.Substring(0, 2) : prefab.name.Substring(0, Mathf.Min(2, prefab.name.Length));
    }

    private static Color EnemyColor(EntityTurn prefab) {
        if (prefab == null) return Color.magenta;
        float hue = Mathf.Repeat(prefab.name.Aggregate(0, (h, c) => h * 31 + c) * 0.618f, 1f);
        return Color.HSVToRGB(hue, 0.65f, 0.75f);
    }

    private static List<EntityTurn> FindEnemyPrefabs() {
        Dictionary<EntityTurn, int> counts = PlacementCounts();
        return AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Entities" })
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)).GetComponent<EntityTurn>())
            // The base Enemy prefab has no EntityData: not a real enemy
            .Where(entity => entity != null && entity is not PlayerTurn && entity.TryGetComponent(out EntityStats stats) && stats.Data != null)
            .OrderByDescending(entity => counts.GetValueOrDefault(entity))
            .ThenBy(entity => entity.name)
            .ToList();
    }

    // How many times each enemy is placed across the layouts: the palette starts with the most common one
    private static Dictionary<EntityTurn, int> PlacementCounts() =>
        AssetDatabase.FindAssets("t:RoomLayout")
            .Select(guid => AssetDatabase.LoadAssetAtPath<RoomLayout>(AssetDatabase.GUIDToAssetPath(guid)))
            .SelectMany(layout => layout.enemyLayouts).SelectMany(layout => layout.enemies)
            .Where(enemy => enemy.prefab != null)
            .GroupBy(enemy => enemy.prefab).ToDictionary(g => g.Key, g => g.Count());
}
