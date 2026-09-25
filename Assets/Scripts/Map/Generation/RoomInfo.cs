using UnityEngine;

/// <summary>
/// Stores info about a room composing a map
/// </summary>
public class RoomInfo
{
	private Room roomPrefab;
	//Kept deactivated once left, and shown again as it was when the player comes back
	private Room loadedRoom;

	private bool alreadyVisited;
	private int layoutIndex;

	/// <param name="roomPrefab">The prefab that will be used when loading this room</param>
	/// <param name="layoutIndex">The room's layout that will be used when loading this room</param>
    public RoomInfo(Room roomPrefab, int layoutIndex) {
		this.roomPrefab = roomPrefab;
		this.layoutIndex = layoutIndex;
		alreadyVisited = false;
	}

	/// <summary>
	/// Loads the corresponding room using the chosen spawnLayout if it's the first time the room is visited,
	/// removing the exits leading nowhere and adding <paramref name="exitVFX"/> on the others.
	/// A room visited before is reactivated as the player left it
	/// </summary>
	public Room LoadRoom(System.Func<Direction, bool> hasExit, GameObject exitVFX) {
		if (loadedRoom != null) {
			loadedRoom.gameObject.SetActive(true);
			loadedRoom.enabled = true;
		}
		else {
			loadedRoom = Object.Instantiate(roomPrefab);
			loadedRoom.SetExits(hasExit, exitVFX);
		}
		loadedRoom.Init(this);
		alreadyVisited = true;
		return loadedRoom;
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
