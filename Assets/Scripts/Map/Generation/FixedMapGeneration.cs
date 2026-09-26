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
		Vector2Int maxPosition = new Vector2Int(0, 0);
		foreach (Vector2Int position in positions) maxPosition = Vector2Int.Max(maxPosition, position);
		List<List<RoomInfo>> mapLayout = MapGeneration.Grid<RoomInfo>(maxPosition.x + 1, maxPosition.y + 1, null);

		for(int i = 0; i < positions.Count; ++i) {
			mapLayout[positions[i].x][positions[i].y] = new RoomInfo(rooms[i], layoutIndexes[i]);
		}


		return mapLayout;
	}

	public Vector2Int GetSpawnPosition() {
		return spawnPosition;
	}
}
