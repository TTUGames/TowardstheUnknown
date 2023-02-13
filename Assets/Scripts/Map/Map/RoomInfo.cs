using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores info about a room composing a map
/// </summary>
public class RoomInfo
{
	private Room roomPrefab;

	private bool alreadyVisited;
	private int layoutIndex;
	public List<Artifact> remainingOrbLoot;

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
		Room room = GameObject.Instantiate<Room>(roomPrefab);
		if (roomPrefab.type != RoomType.ANTECHAMBER && roomPrefab.type != RoomType.BOSS) {
			AkSoundEngine.PostEvent("SwitchGameplay", room.gameObject);
			if (roomPrefab.type == RoomType.COMBAT && alreadyVisited == false && layoutIndex != -1) {
				AkSoundEngine.PostEvent("SwitchCombat", room.gameObject);
			}
		}
		else {
			if (roomPrefab.type == RoomType.ANTECHAMBER) {
				AkSoundEngine.PostEvent("SwitchExplore", room.gameObject);
				AkSoundEngine.PostEvent("SwitchBoss", room.gameObject);
			}
			else if (alreadyVisited == false && layoutIndex != -1 && roomPrefab.type == RoomType.BOSS) {
				AkSoundEngine.PostEvent("SwitchCombat", room.gameObject);
				AkSoundEngine.PostEvent("BossPhase1", room.gameObject);
			}
		}
		room.SetExits(hasNorthExit, hasSouthExit, hasEastExit, hasWestExit);

		room.Init(this);

		alreadyVisited = true;

		return room;
	}

	public RoomType GetRoomType() {
		return roomPrefab.type;
	}

	public bool IsAlreadyVisited() {
		return alreadyVisited;
	}

	public int GetLayoutIndex() {
		return alreadyVisited ? -1 : layoutIndex;
	}
}
