using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPool
{
	private Dictionary<int, List<Pair<Room, int>>> roomLayoutsByDifficulty;
    public RoomPool(string folderPath) {
		roomLayoutsByDifficulty = new Dictionary<int, List<Pair<Room, int>>>();
		foreach(Room room in Resources.LoadAll<Room>(folderPath)) {
			List<EnemySpawnLayout> layouts = new List<EnemySpawnLayout>(room.GetComponentsInChildren<EnemySpawnLayout>());
			for (int layoutIndex = 0; layoutIndex < layouts.Count; ++layoutIndex) {
				int layoutDifficulty = layouts[layoutIndex].difficulty;
				if (!roomLayoutsByDifficulty.ContainsKey(layoutDifficulty)) 
					roomLayoutsByDifficulty.Add(layoutDifficulty, new List<Pair<Room, int>>());

				roomLayoutsByDifficulty[layoutDifficulty].Add(new Pair<Room, int>(room, layoutIndex));
			}
		}
	}

	public RoomInfo GetRoom(int difficulty) {
		List<Pair<Room, int>> possibleRooms = roomLayoutsByDifficulty[difficulty];
		Pair<Room, int> room = possibleRooms[Random.Range(0, possibleRooms.Count)];
		return new RoomInfo(room.first, room.second);
	}
}
