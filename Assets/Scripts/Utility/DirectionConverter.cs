using UnityEngine;

/// <summary>
/// A utility static class used to convert a vector to a cardinal direction, and vice-versa
/// </summary>
public static class DirectionConverter
{
    public static Vector2Int DirToVect(Direction direction) {
		switch (direction) {
			case Direction.NORTH:
				return Vector2Int.up;
			case Direction.SOUTH:
				return Vector2Int.down;
			case Direction.WEST:
				return Vector2Int.left;
			case Direction.EAST:
				return Vector2Int.right;
		}
		throw new System.Exception("Cannot convert " + direction + " to a Vector2");
	}

	public static Direction GetOppositeDirection(Direction direction) {
		switch(direction) {
			case Direction.NORTH:
				return Direction.SOUTH;
			case Direction.SOUTH:
				return Direction.NORTH;
			case Direction.EAST:
				return Direction.WEST;
			case Direction.WEST:
				return Direction.EAST;
		}
		throw new System.Exception("Cannot find opposite direction for " + direction);
	}
}
