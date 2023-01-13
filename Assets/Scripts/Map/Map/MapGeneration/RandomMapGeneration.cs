using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMapGeneration : MonoBehaviour, MapGeneration
{
	[SerializeField] private int totalDifficulty;
	[SerializeField] private Vector2Int maxSize;
	[SerializeField] private int treasureRoomQuantity;
	[SerializeField] private int combatRoomQuantity;
	[SerializeField] private int minCombatRoomDifficulty;
	[SerializeField] private int maxCombatRoomDifficulty;
	[SerializeField] private int distanceToBossRoom;

	[SerializeField] private string spawnRoomFolderPath = "Prefabs/Rooms/SpawnRooms";
	[SerializeField] private string combatRoomFolderPath = "Prefabs/Rooms/CombatRooms";
	[SerializeField] private string treasureRoomFolderPath = "Prefabs/Rooms/TreasureRooms";
	[SerializeField] private string bossRoomFolderPath = "Prefabs/Rooms/SpawnRooms";
	[SerializeField] private string antechamberRoomFolderPath = "Prefabs/Rooms/SpawnRooms";

	private enum RoomType { UNDEFINED, EMPTY, SPAWN, COMBAT, ANTECHAMBER, TREASURE, BOSS }

	private List<List<RoomType>> mapLayout;
	private Vector2Int spawnPosition;

	private void CheckValues() {
		if (maxSize.x < 3 || maxSize.y < 3) 
			throw new System.Exception("Map size is too small");
		if (combatRoomQuantity >= (maxSize.x - 2) * (maxSize.y - 2)) 
			throw new System.Exception("Requesting too many combatrooms for the given maxSize");
		if (totalDifficulty < minCombatRoomDifficulty * combatRoomQuantity || totalDifficulty > maxCombatRoomDifficulty * combatRoomQuantity)
			throw new System.Exception("Total difficulty cannot be reached with given combat room quantity and difficulty range");
		if (distanceToBossRoom > combatRoomQuantity)
			throw new System.Exception("Not enough combat rooms for given distance to boss room");
		if (distanceToBossRoom > maxSize.x + maxSize.y - 4)
			throw new System.Exception("Map not big enough for given distance to boss room");
	}

	private void Init() {
		mapLayout = new List<List<RoomType>>();
		for (int x = 0; x < maxSize.x; ++x) {
			mapLayout.Add(new List<RoomType>());
			for (int y = 0; y < maxSize.y; ++y) {
				mapLayout[x].Add(RoomType.UNDEFINED);
			}
		}
	}

	private List<Vector2Int> GetAdjacentPositions(Vector2Int position) {
		List<Vector2Int> adjacentPositions = new List<Vector2Int>();
		foreach (Vector2Int direction in new List<Vector2Int> { Vector2Int.right, Vector2Int.left, Vector2Int.down, Vector2Int.up }) {
			Vector2Int adjacentPosition = position + direction;
			if (adjacentPosition.x < 0 || adjacentPosition.x >= maxSize.x) continue;
			if (adjacentPosition.y < 0 || adjacentPosition.y >= maxSize.y) continue;
			adjacentPositions.Add(adjacentPosition);
		}
		return adjacentPositions;
	}

	/// <summary>
	/// Sets the Antechamber position in a random tile (excluding borders)
	/// The chosen position is guaranted to allow the creation of a path of length distanceToBossRoom in the criticalPathDirection
	/// </summary>
	/// <param name="criticalPathDirection"></param>
	/// <returns></returns>
	private Vector2Int SetAntechamberPosition(Vector2Int criticalPathDirection) {
		Vector2Int antechamberPosition = new Vector2Int(
			criticalPathDirection.x == 1 ? 1 : maxSize.x - 2,
			criticalPathDirection.y == 1 ? 1 : maxSize.y - 2
		);
		int totalOffset = Random.Range(0, maxSize.x + maxSize.y - 4 - distanceToBossRoom);
		int xOffset = Random.Range(0, totalOffset + 1);
		int yOffset = totalOffset - xOffset;

		antechamberPosition.x += xOffset * criticalPathDirection.x;
		antechamberPosition.y += yOffset * criticalPathDirection.y;

		mapLayout[antechamberPosition.x][antechamberPosition.y] = RoomType.ANTECHAMBER;

		return antechamberPosition;
	}

	/// <summary>
	/// Sets the bossRoom position adjacent to the antechamber position, in the opposite direction of criticalPathDirection
	/// </summary>
	/// <param name="criticalPathDirection"></param>
	/// <param name="antechamberPosition"></param>
	/// <returns></returns>
	private Vector2Int SetBossRoomPosition(Vector2Int criticalPathDirection, Vector2Int antechamberPosition) {
		Vector2Int bossRoomPosition = antechamberPosition;
		if (Random.Range(0, 2) == 0)
			bossRoomPosition.x -= criticalPathDirection.x;
		else
			bossRoomPosition.y -= criticalPathDirection.y;

		mapLayout[bossRoomPosition.x][bossRoomPosition.y] = RoomType.BOSS;

		foreach(Vector2Int adjacentPosition in GetAdjacentPositions(bossRoomPosition)) {
			if (mapLayout[adjacentPosition.x][adjacentPosition.y] == RoomType.UNDEFINED) {
				mapLayout[adjacentPosition.x][adjacentPosition.y] = RoomType.EMPTY;
			}
		}

		return bossRoomPosition;
	}

	/// <summary>
	/// Creates the critical path of the room generation
	/// The critical path is a path composed of a spawn, distanceToBossRoom combat rooms, the antechamber and the boss room
	/// The direction of the critical path if always the same for a same generation. It can be northwest, southwest, northeast, northwest, but will never backtrack
	/// </summary>
	private void SetCriticalPath() {
		Vector2Int criticalPathDirection = new Vector2Int(Random.Range(0, 2) == 0 ? 1 : -1, Random.Range(0, 2) == 0 ? 1 : -1);

		Vector2Int antechamberPosition = SetAntechamberPosition(criticalPathDirection);
		Vector2Int bossRoomPosition = SetBossRoomPosition(criticalPathDirection, antechamberPosition);

		Vector2Int currentPosition = antechamberPosition;
		for (int dist = 0; dist < distanceToBossRoom; ++dist) {
			if (currentPosition.x != 0 && currentPosition.x != maxSize.x &&
				(currentPosition.y == 0 || currentPosition.y == maxSize.y - 1 || Random.Range(0, 2) == 0))
				currentPosition.x += criticalPathDirection.x;
			else
				currentPosition.y += criticalPathDirection.y;
			mapLayout[currentPosition.x][currentPosition.y] = RoomType.COMBAT;
		}
		mapLayout[currentPosition.x][currentPosition.y] = RoomType.SPAWN;
	}

	/*
	/// <summary>
	/// Calls GenerateRoomDifficulty 1000 times and counts how many times each result happened
	/// </summary>
	private void Test() {
		Dictionary<List<int>, int> results = new Dictionary<List<int>, int>();
		for (int step = 0; step < 1000; ++step) {
			List<int> result = GenerateRoomDifficultyList();
			bool found = false;
			foreach(List<int> r in results.Keys) {
				bool areEqual = true;
				for(int i = 0; i < r.Count; ++i) {
					if (r[i] != result[i]) {
						areEqual = false;
						break;
					}
				}
				if (areEqual) {
					found = true;
					results[r] += 1;
					break;
				}
			}
			if (!found) {
				results.Add(result, 1);
			}
		}
		foreach (List<int> result in results.Keys) {
			string display = "";
			foreach (int i in result) display += i + " ";
			display += "FOUND " + results[result] + " TIMES";
			Debug.Log(display);
		}
	}*/

	/// <summary>
	/// Distributes difficulty among combatRoomQuantity rooms for a total of totalDifficulty.
	/// Each room gets a difficulty from minCombatRoomDifficulty and maxCombatRoomDifficulty.
	/// All the distributions are not equiprobable, but the most probable are the more diverse ones
	/// </summary>
	/// <returns>A sorted list containing the difficulties</returns>
	private List<int> GenerateRoomDifficultyList() {
		List<int> roomDifficultyList = new List<int>();
		int baseDifficulty = totalDifficulty / combatRoomQuantity;
		int additionnalDifficulty = totalDifficulty % combatRoomQuantity;
		for (int i = 0; i < combatRoomQuantity - additionnalDifficulty; ++i) {
			roomDifficultyList.Add(baseDifficulty);
		}
		for (int i = combatRoomQuantity - additionnalDifficulty; i < combatRoomQuantity; ++i) {
			roomDifficultyList.Add(baseDifficulty + 1);
		}

		for (int firstIndex = 0; firstIndex < roomDifficultyList.Count; ++firstIndex) {
			int secondIndex = Random.Range(0, roomDifficultyList.Count);
			int maxOffset = Mathf.Min(maxCombatRoomDifficulty - roomDifficultyList[secondIndex], roomDifficultyList[firstIndex] - minCombatRoomDifficulty);
			int offset = Random.Range(0, maxOffset + 1);
			roomDifficultyList[firstIndex] -= offset;
			roomDifficultyList[secondIndex] += offset;
		}

		roomDifficultyList.Sort();

		/*string roomDifficultyString = "";
		foreach (int i in roomDifficultyList) {
			roomDifficultyString += i + " ";
		}
		Debug.Log(roomDifficultyString);*/
		return roomDifficultyList;
	}


	public List<List<RoomInfo>> Generate() {
		CheckValues();

		Init();

		SetCriticalPath();

		string displayAsString = "";
		foreach(List<RoomType> row in mapLayout) {
			foreach(RoomType type in row) {
				switch (type) {
					case RoomType.UNDEFINED: 
						displayAsString += '-';
						break;
					case RoomType.EMPTY: 
						displayAsString += 'E';
						break;
					case RoomType.COMBAT: 
						displayAsString += 'C';
						break;
					case RoomType.SPAWN: 
						displayAsString += 'S';
						break;
					case RoomType.ANTECHAMBER: 
						displayAsString += 'A';
						break;
					case RoomType.BOSS: 
						displayAsString += 'B';
						break;
					case RoomType.TREASURE: 
						displayAsString += 'T';
						break;
				}
			}
			displayAsString += "\n";
		}
		Debug.Log(displayAsString);

		return null;
	}

	public Vector2Int GetSpawnPosition() {
		return spawnPosition;
	}
}
