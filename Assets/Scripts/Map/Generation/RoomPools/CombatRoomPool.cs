using System.Collections.Generic;
using UnityEngine;

public class CombatRoomPool
{
	private Dictionary<int, List<(Room room, int layoutIndex)>> unusedRoomLayoutsByDifficulty; //Difficulty -> (room, layoutIndex)
	private Dictionary<int, List<(Room room, int layoutIndex)>> usedRooms;
    public CombatRoomPool(IEnumerable<Room> rooms) {
		usedRooms = new Dictionary<int, List<(Room room, int layoutIndex)>>();
		unusedRoomLayoutsByDifficulty = new Dictionary<int, List<(Room room, int layoutIndex)>>();
		foreach(Room room in rooms) {
			List<RoomLayout.EnemyLayout> layouts = room.Layout.enemyLayouts;
			for (int layoutIndex = 0; layoutIndex < layouts.Count; ++layoutIndex) {
				int layoutDifficulty = layouts[layoutIndex].difficulty;
				if (!unusedRoomLayoutsByDifficulty.ContainsKey(layoutDifficulty)) {
					unusedRoomLayoutsByDifficulty.Add(layoutDifficulty, new List<(Room room, int layoutIndex)>());
					usedRooms.Add(layoutDifficulty, new List<(Room room, int layoutIndex)>());
				}

				unusedRoomLayoutsByDifficulty[layoutDifficulty].Add((room, layoutIndex));
			}
		}
	}

	public RoomInfo GetRoom(int difficulty) {
		if (!unusedRoomLayoutsByDifficulty.TryGetValue(difficulty, out List<(Room room, int layoutIndex)> possibleRooms))
			throw new System.Exception($"No combat room has an enemy layout of difficulty {difficulty}");
		if (possibleRooms.Count == 0) {
			unusedRoomLayoutsByDifficulty[difficulty] = usedRooms[difficulty];
			usedRooms[difficulty] = new List<(Room room, int layoutIndex)>();
			possibleRooms = unusedRoomLayoutsByDifficulty[difficulty];
		}

		(Room room, int layoutIndex) picked = possibleRooms[Random.Range(0, possibleRooms.Count)];

		possibleRooms.Remove(picked);
		usedRooms[difficulty].Add(picked);

		return new RoomInfo(picked.room, picked.layoutIndex);
	}
}
