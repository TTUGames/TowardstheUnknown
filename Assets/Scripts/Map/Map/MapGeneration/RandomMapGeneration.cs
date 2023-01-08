using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMapGeneration : MonoBehaviour, MapGeneration
{
	[SerializeField] private int difficulty;
	[SerializeField] private Vector2Int maxSize;
	[SerializeField] private int treasureRoomQuantity;
	[SerializeField] private int combatRoomQuantity;

	[SerializeField] private string spawnRoomFolderPath = "Prefabs/Rooms/SpawnRooms";
	[SerializeField] private string combatRoomFolderPath = "Prefabs/Rooms/CombatRooms";
	[SerializeField] private string treasureRoomFolderPath = "Prefabs/Rooms/TreasureRooms";
	//[SerializeField] private string bossRoomFolderPath;

	private List<List<RoomInfo>> mapLayout;
	private List<Vector2Int> availablePositions; //Positions of empty rooms adjacent to filled rooms
	private Vector2Int spawnPosition;

	private void CheckValues() {
		if (maxSize.x < 3 || maxSize.y < 3) throw new System.Exception("Map size is too small");
		if (combatRoomQuantity >= (maxSize.x - 2) * (maxSize.y - 2)) throw new System.Exception("Requesting too many combatrooms for the given maxSize");
	}

	private void Init() {
		mapLayout = new List<List<RoomInfo>>();
		availablePositions = new List<Vector2Int>();
		for (int x = 0; x < maxSize.x; ++x) {
			mapLayout.Add(new List<RoomInfo>());
			for (int y = 0; y < maxSize.y; ++y) {
				mapLayout[x].Add(null);
			}
		}
	}

	private void SetRoomInfo(Vector2Int position, RoomInfo roomInfo) {
		mapLayout[position.x][position.y] = roomInfo;
		if (availablePositions.Contains(position)) availablePositions.Remove(position);
		foreach(Vector2Int direction in new List<Vector2Int> { Vector2Int.right, Vector2Int.left, Vector2Int.down, Vector2Int.up }) {
			Vector2Int adjacentPosition = position + direction;
			if (adjacentPosition.x < 0 || adjacentPosition.x >= maxSize.x) continue;
			if (adjacentPosition.y < 0 || adjacentPosition.y >= maxSize.y) continue;
			if (mapLayout[adjacentPosition.x][adjacentPosition.y] != null) continue;
			availablePositions.Add(adjacentPosition);
		}
	}

	private void SetSpawnRoom() {
		GenericRoomPool spawnRoomPool = new GenericRoomPool(spawnRoomFolderPath);
		spawnPosition = new Vector2Int(Random.Range(1, maxSize.x - 1), Random.Range(1, maxSize.y - 1));
		SetRoomInfo(spawnPosition, spawnRoomPool.GetRoom());
	}

	private void GenerateCombatRooms() {
		CombatRoomPool combatRoomPool = new CombatRoomPool(combatRoomFolderPath);
		for (int i = 0; i < combatRoomQuantity; ++i) {
			Vector2Int position = availablePositions[Random.Range(0, availablePositions.Count)];
			SetRoomInfo(position, combatRoomPool.GetRoom(1));
		}
	}



	public List<List<RoomInfo>> Generate() {
		CheckValues();

		Init();

		SetSpawnRoom();

		GenerateCombatRooms();

		string layoutAsString = "";
		foreach(List<RoomInfo> row in mapLayout) {
			foreach(RoomInfo room in row) {
				if (room == null) layoutAsString += "-";
				else if (room == mapLayout[spawnPosition.x][spawnPosition.y]) layoutAsString += "0";
				else layoutAsString += "x";
			}
			layoutAsString += "\n";
		}
		Debug.Log(layoutAsString);

		return mapLayout;
	}

	public Vector2Int GetSpawnPosition() {
		return spawnPosition;
	}
}
