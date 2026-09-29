using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The gameplay layer of a room on a <see cref="Size"/> x <see cref="Size"/> grid: its terrain, exits, deploy cells and enemy
/// layouts. The terrain is baked into the room's tiles in the editor (Room Layout inspector); the deploy cells and the enemies
/// are read at runtime. Cell (0, 0) is the south west corner, x goes east and y north
/// </summary>
[CreateAssetMenu(fileName = "RoomLayout", menuName = "TTU/Room Layout")]
public class RoomLayout : ScriptableObject
{
    public const int Size = 15;

    public enum Cell {
        /// <summary>No tile</summary>
        Empty,
        /// <summary>A walkable tile</summary>
        Floor,
        /// <summary>Blocks the walk but not the line of sight: an invisible tile at the floor's height</summary>
        Hole,
        /// <summary>Blocks the walk and the line of sight: an invisible tile taller than the floor</summary>
        Wall,
        /// <summary>A walkable tile leading to the adjacent room, in <see cref="RoomLayout.ExitDirection"/></summary>
        Exit,
    }

    [System.Serializable]
    public class EnemyPlacement {
        public EntityTurn prefab;
        public Vector2Int cell;
        [Tooltip("From the top of the cell's tile, in the room's space: an entity bigger than a tile stands between cells")]
        public Vector3 offset;
    }

    [System.Serializable]
    public class EnemyLayout {
        public string name = "Layout";
        [Tooltip("The random generation splits the map's total difficulty across the combat rooms' layouts")]
        public int difficulty = 1;
        public List<EnemyPlacement> enemies = new List<EnemyPlacement>();
    }

    [System.Serializable]
    public struct Exit {
        public Vector2Int cell;
        public Direction direction;
    }

    [Tooltip("The room's local position of the center of cell (0, 0)'s tile")]
    public Vector3 origin;

    [Tooltip("The height of the walls' tiles, relative to the floor's: over 1, they block the lines of sight")]
    public float wallHeight = 2f;

    [SerializeField, HideInInspector] private Cell[] cells = new Cell[Size * Size];

    [HideInInspector] public List<Exit> exits = new List<Exit>();

    [Tooltip("The tiles the player chooses from before a fight, the first one by default")]
    [HideInInspector] public List<Vector2Int> deployCells = new List<Vector2Int>();

    [HideInInspector] public List<EnemyLayout> enemyLayouts = new List<EnemyLayout>();

    public static bool InGrid(Vector2Int cell) => cell.x >= 0 && cell.y >= 0 && cell.x < Size && cell.y < Size;

    public Cell Get(Vector2Int cell) => InGrid(cell) ? Cells[cell.y * Size + cell.x] : Cell.Empty;

    public void Set(Vector2Int cell, Cell value) {
        if (InGrid(cell)) Cells[cell.y * Size + cell.x] = value;
    }

    private Cell[] Cells {
        get {
            if (cells == null || cells.Length != Size * Size) System.Array.Resize(ref cells, Size * Size);
            return cells;
        }
    }

    /// <summary>
    /// The direction of the exit on this cell, <see cref="Direction.NULL"/> if none
    /// </summary>
    public Direction ExitDirection(Vector2Int cell) {
        foreach (Exit exit in exits)
            if (exit.cell == cell) return exit.direction;
        return Direction.NULL;
    }

    /// <summary>
    /// The room's local position of a cell's tile
    /// </summary>
    public Vector3 CellToLocal(Vector2Int cell) => origin + new Vector3(cell.x, 0, cell.y);

    /// <summary>
    /// The cell under a room's local position
    /// </summary>
    public Vector2Int LocalToCell(Vector3 local) =>
        new Vector2Int(Mathf.RoundToInt(local.x - origin.x), Mathf.RoundToInt(local.z - origin.z));
}
