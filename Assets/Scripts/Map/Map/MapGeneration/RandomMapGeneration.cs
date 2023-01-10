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

	[SerializeField] private string spawnRoomFolderPath = "Prefabs/Rooms/SpawnRooms";
	[SerializeField] private string combatRoomFolderPath = "Prefabs/Rooms/CombatRooms";
	[SerializeField] private string treasureRoomFolderPath = "Prefabs/Rooms/TreasureRooms";
	//[SerializeField] private string bossRoomFolderPath;

	private List<List<RoomInfo>> mapLayout;
	private List<List<char>> display;
	private List<Vector2Int> availablePositions; //Positions of empty rooms adjacent to filled rooms
	private Vector2Int spawnPosition;

	private void CheckValues() {
		if (maxSize.x < 3 || maxSize.y < 3) 
			throw new System.Exception("Map size is too small");
		if (combatRoomQuantity >= (maxSize.x - 2) * (maxSize.y - 2)) 
			throw new System.Exception("Requesting too many combatrooms for the given maxSize");
		if (totalDifficulty < minCombatRoomDifficulty * combatRoomQuantity || totalDifficulty > maxCombatRoomDifficulty * combatRoomQuantity)
			throw new System.Exception("Total difficulty cannot be reached with given combat room quantity and difficulty range");
	}

	private void Init() {
		mapLayout = new List<List<RoomInfo>>();
		display = new List<List<char>>();
		availablePositions = new List<Vector2Int>();
		for (int x = 0; x < maxSize.x; ++x) {
			mapLayout.Add(new List<RoomInfo>());
			display.Add(new List<char>());
			for (int y = 0; y < maxSize.y; ++y) {
				mapLayout[x].Add(null);
				display[x].Add('-');
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
		display[spawnPosition.x][spawnPosition.y] = 'S';
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

	private void GenerateCombatRooms() {
		List<int> difficultyList = GenerateRoomDifficultyList();
		CombatRoomPool combatRoomPool = new CombatRoomPool(combatRoomFolderPath);
		for (int i = 0; i < combatRoomQuantity; ++i) {
			Vector2Int position = availablePositions[Random.Range(0, availablePositions.Count)];
			SetRoomInfo(position, combatRoomPool.GetRoom(difficultyList[i]));
			display[position.x][position.y] = (char)(difficultyList[i]+48);
		}
	}

	public List<List<RoomInfo>> Generate() {
		CheckValues();

		Init();

		SetSpawnRoom();

		GenerateCombatRooms();

		string displayAsString = "";
		foreach(List<char> row in display) {
			foreach(char c in row) {
				displayAsString += c;
			}
			displayAsString += "\n";
		}
		Debug.Log(displayAsString);

		return mapLayout;
	}

	public Vector2Int GetSpawnPosition() {
		return spawnPosition;
	}
}
