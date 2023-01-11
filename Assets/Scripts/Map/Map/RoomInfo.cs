using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores info about a room composing a map
/// </summary>
public class RoomInfo
{
	private Room roomPrefab;
	private int layoutIndex;
	private bool alreadyVisited;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="roomPrefab">The prefab that will be used when loading this room</param>
	/// <param name="layoutIndex">The room's layout that will be used when loading this room</param>
    public RoomInfo(Room roomPrefab, int layoutIndex) {
		this.roomPrefab = roomPrefab;
		this.layoutIndex = layoutIndex;
		alreadyVisited = false;
	}

	/// <summary>
	/// Loads the corresponding room using the chosen spawnLayout if it's the first time the room is visited.
	/// Also disables the exits depending on the parameters.
	/// </summary>
	/// <param name="hasNorthExit"></param>
	/// <param name="hasSouthExit"></param>
	/// <param name="hasEastExit"></param>
	/// <param name="hasWestExit"></param>
	/// <returns></returns>
	public Room LoadRoom(bool hasNorthExit, bool hasSouthExit, bool hasEastExit, bool hasWestExit) {
		if (alreadyVisited == false && layoutIndex != -1 && roomPrefab.GetComponent<CombatPlayerDeploy>() != null) {
			AkSoundEngine.PostEvent("SwitchCombat", GameObject.FindObjectOfType<Map>().gameObject);
		}
		Room room = GameObject.Instantiate<Room>(roomPrefab);
		room.SetExits(hasNorthExit, hasSouthExit, hasEastExit, hasWestExit);

		room.Init(alreadyVisited ? - 1 : layoutIndex);

		alreadyVisited = true;

		return room;
	}
}
