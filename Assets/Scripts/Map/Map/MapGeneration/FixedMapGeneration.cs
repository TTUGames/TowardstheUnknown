using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedMapGeneration : MonoBehaviour, MapGeneration {
	[SerializeField] List<Vector2Int> positions;
	[SerializeField] List<Room> rooms;
	[SerializeField] List<int> layoutIndexes;
	[SerializeField] protected Vector2Int spawnPosition;

	/// <summary>
	/// Generates the room using positions, roomprefabs and layout indexes set in inspector
	/// </summary>
	/// <returns></returns>
	public List<List<RoomInfo>> Generate() {
		List<List<RoomInfo>> mapLayout = new List<List<RoomInfo>>();

		Vector2Int maxPosition = new Vector2Int(0, 0);

		foreach(Vector2Int position in positions) {
			if (position.x > maxPosition.x) maxPosition.x = position.x;
			if (position.y > maxPosition.y) maxPosition.y = position.y;
		}

		for (int x = 0; x <= maxPosition.x; ++x) {
			mapLayout.Add(new List<RoomInfo>());
			for (int y = 0; y <= maxPosition.y; ++y) {
				mapLayout[x].Add(null);
			}
		}


		for(int i = 0; i < positions.Count; ++i) {
			mapLayout[positions[i].x][positions[i].y] = new RoomInfo(rooms[i], layoutIndexes[i]);
		}


		return mapLayout;
	}

	public Vector2Int GetSpawnPosition() {
		return spawnPosition;
	}
}
