using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatRoomPool
{
	private Dictionary<int, List<Pair<Room, int>>> unusedRoomLayoutsByDifficulty; //Difficulty -> Pair<Room, layoutIndex>
	private Dictionary<int, List<Pair<Room, int>>> usedRooms;
    public CombatRoomPool(string folderPath) {
		usedRooms = new Dictionary<int, List<Pair<Room, int>>>();
		unusedRoomLayoutsByDifficulty = new Dictionary<int, List<Pair<Room, int>>>();
		foreach(Room room in Resources.LoadAll<Room>(folderPath)) {
			List<EnemySpawnLayout> layouts = new List<EnemySpawnLayout>(room.GetComponentsInChildren<EnemySpawnLayout>());
			for (int layoutIndex = 0; layoutIndex < layouts.Count; ++layoutIndex) {
				int layoutDifficulty = layouts[layoutIndex].difficulty;
				if (!unusedRoomLayoutsByDifficulty.ContainsKey(layoutDifficulty)) 
					unusedRoomLayoutsByDifficulty.Add(layoutDifficulty, new List<Pair<Room, int>>());

				unusedRoomLayoutsByDifficulty[layoutDifficulty].Add(new Pair<Room, int>(room, layoutIndex));
			}
		}
	}

	public RoomInfo GetRoom(int difficulty) {
		List<Pair<Room, int>> possibleRooms = unusedRoomLayoutsByDifficulty[difficulty];
		if (possibleRooms.Count == 0) {
			unusedRoomLayoutsByDifficulty[difficulty] = usedRooms[difficulty];
			usedRooms[difficulty] = new List<Pair<Room, int>>();
			possibleRooms = unusedRoomLayoutsByDifficulty[difficulty];
		}

		Pair<Room, int> room = possibleRooms[Random.Range(0, possibleRooms.Count)];

		possibleRooms.Remove(room);
		if (!usedRooms.ContainsKey(difficulty))
			usedRooms.Add(difficulty, new List<Pair<Room, int>>());
		usedRooms[difficulty].Add(room);

		return new RoomInfo(room.first, room.second);
	}
}
