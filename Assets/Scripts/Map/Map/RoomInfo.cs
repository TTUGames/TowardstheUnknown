using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfo
{
	private Room roomPrefab;
	private int layoutIndex;
	private bool alreadyVisited;

    public RoomInfo(Room roomPrefab, int layoutIndex) {
		this.roomPrefab = roomPrefab;
		this.layoutIndex = layoutIndex;
		alreadyVisited = false;
	}

	public Room LoadRoom(bool hasNorthExit, bool hasSouthExit, bool hasEastExit, bool hasWestExit) {
		
		Room room = GameObject.Instantiate<Room>(roomPrefab);
		room.SetExits(hasNorthExit, hasSouthExit, hasEastExit, hasWestExit);

		room.Init();
		if (!alreadyVisited) room.LoadSpawnLayout(layoutIndex);
		room.StartCoroutine(room.EndRoomInit());

		alreadyVisited = true;

		return room;
	}
}
