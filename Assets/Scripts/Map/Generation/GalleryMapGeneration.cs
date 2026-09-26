using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Lays every room of a room set in a row, west to east, without enemies: a walk through all the rooms to check them one
/// after the other. The spawn rooms come first and the boss rooms, which have no exit, last.
/// </summary>
public class GalleryMapGeneration : MonoBehaviour, MapGeneration
{
	[SerializeField, Tooltip("The rooms shown, the game's set so that a new room shows up")] private RoomSet rooms;
	[SerializeField, Tooltip("A dead end: the boss rooms have no exit")] private bool includeBossRooms = true;

	// No spawn layout: the rooms load without enemies, their exits open
	private const int NoEnemies = -1;

	public List<List<RoomInfo>> Generate()
	{
		IEnumerable<Room> row = rooms.spawnRooms.Concat(rooms.antechamberRooms).Concat(rooms.treasureRooms).Concat(rooms.combatRooms);
		if (includeBossRooms) row = row.Concat(rooms.bossRooms);
		//One column per room, each holding a single room
		return row.Where(room => room != null).Distinct()
			.Select(room => new List<RoomInfo> { new RoomInfo(room, NoEnemies) }).ToList();
	}

	public Vector2Int GetSpawnPosition() => Vector2Int.zero;
}
