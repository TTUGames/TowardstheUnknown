using System.Collections;
using System.Collections.Generic;
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
		return Vector2Int.zero;
	}

	public static Direction VectToDir(Vector2Int vector) {
		if (vector == Vector2Int.up) return Direction.NORTH;
		if (vector == Vector2Int.down) return Direction.SOUTH;
		if (vector == Vector2Int.left) return Direction.WEST;
		if (vector == Vector2Int.right) return Direction.EAST;
		throw new System.Exception("Can't convert " + vector + " to a Direction");
	}
}
