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

	/// <param name="roomPrefab">The prefab that will be used when loading this room</param>
	/// <param name="layoutIndex">The room's layout that will be used when loading this room</param>
    public RoomInfo(Room roomPrefab, int layoutIndex) {
		this.roomPrefab = roomPrefab;
		this.layoutIndex = layoutIndex;
		alreadyVisited = false;
	}

	/// <summary>
	/// Loads the corresponding room using the chosen spawnLayout if it's the first time the room is visited.
	/// Also removes the exits leading nowhere and adds <paramref name="exitVFX"/> on the others.
	/// </summary>
	public Room LoadRoom(System.Func<Direction, bool> hasExit, GameObject exitVFX) {
		Room room = Object.Instantiate(roomPrefab);
		PlayMusic(room.gameObject);
		room.SetExits(hasExit, exitVFX);
		room.Init(this);
		alreadyVisited = true;
		return room;
	}

	/// <summary>
	/// Switches the music depending on the room type and if a fight is going to start
	/// </summary>
	private void PlayMusic(GameObject room) {
		bool startsFight = !alreadyVisited && layoutIndex != -1;
		switch (roomPrefab.type) {
			case RoomType.ANTECHAMBER:
				AkUnitySoundEngine.PostEvent("SwitchExplore", room);
				AkUnitySoundEngine.PostEvent("SwitchBoss", room);
				break;
			case RoomType.BOSS:
				if (!startsFight) break;
				AkUnitySoundEngine.PostEvent("SwitchCombat", room);
				AkUnitySoundEngine.PostEvent("BossPhase1", room);
				break;
			default:
				AkUnitySoundEngine.PostEvent("SwitchGameplay", room);
				if (roomPrefab.type == RoomType.COMBAT && startsFight)
					AkUnitySoundEngine.PostEvent("SwitchCombat", room);
				break;
		}
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
