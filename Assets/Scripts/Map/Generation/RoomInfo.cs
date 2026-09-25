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
		room.SetExits(hasExit, exitVFX);
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
