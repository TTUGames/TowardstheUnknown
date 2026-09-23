using UnityEngine;

/// <summary>
/// A utility static class used to convert a vector to a cardinal direction, and vice-versa
/// </summary>
public static class DirectionConverter
{
    public static Vector2Int DirToVect(Direction direction) => direction switch {
        Direction.NORTH => Vector2Int.up,
        Direction.SOUTH => Vector2Int.down,
        Direction.WEST => Vector2Int.left,
        Direction.EAST => Vector2Int.right,
        _ => throw new System.Exception("Cannot convert " + direction + " to a Vector2"),
    };

    public static Direction GetOppositeDirection(Direction direction) => direction switch {
        Direction.NORTH => Direction.SOUTH,
        Direction.SOUTH => Direction.NORTH,
        Direction.EAST => Direction.WEST,
        Direction.WEST => Direction.EAST,
        _ => throw new System.Exception("Cannot find opposite direction for " + direction),
    };
}
