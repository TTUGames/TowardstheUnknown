using UnityEngine;

/// <summary>
/// Stores info about a room composing a map
/// </summary>
public class RoomInfo
{
	private Room roomPrefab;
	//Kept deactivated once left, and shown again as it was when the player comes back
	private Room loadedRoom;
	//Made ahead, inactive under the map, while the player is next door: its first load only activates it
	private Room preloaded;

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
	/// Loads the corresponding room with its chosen layout (enemies or treasure) if it's the first time the room is visited,
	/// removing the exits leading nowhere and adding <paramref name="exitVFX"/> on the others.
	/// A room visited before is reactivated as the player left it
	/// </summary>
	public Room LoadRoom(System.Func<Direction, bool> hasExit, GameObject exitVFX) {
		if (loadedRoom != null) {
			loadedRoom.gameObject.SetActive(true);
			loadedRoom.enabled = true;
		}
		else {
			if (preloaded != null) {
				loadedRoom = preloaded;
				preloaded = null;
				loadedRoom.transform.SetParent(null, false);
			}
			else loadedRoom = Object.Instantiate(roomPrefab);
			EditionMaterials.Apply(loadedRoom.gameObject);
			loadedRoom.SetExits(hasExit, exitVFX);
		}
		loadedRoom.Init(this);
		alreadyVisited = true;
		return loadedRoom;
	}

	/// <summary>
	/// Makes the room ahead of its first visit, under <paramref name="holder"/>, an inactive object: its objects wake when it
	/// loads. Nothing if it is already made
	/// </summary>
	/// <returns>Whether it made the room now</returns>
	public bool Preload(Transform holder) {
		if (loadedRoom != null || preloaded != null) return false;
		preloaded = Object.Instantiate(roomPrefab, holder, false);
		return true;
	}

	public RoomType GetRoomType() {
		return roomPrefab.type;
	}

	/// <summary>
	/// A relic lies in the room: one not picked up yet once loaded, or the chest of a treasure room or of the antechamber
	/// before the first visit
	/// </summary>
	public bool HasLoot => loadedRoom != null ? loadedRoom.HasLoot : roomPrefab.type is RoomType.TREASURE or RoomType.ANTECHAMBER;

	/// <summary>
	/// A room of a suspended run visited before it was saved: it loads as left, without its layout (its enemies dead, its
	/// relic taken or lost)
	/// </summary>
	public void MarkVisited() {
		alreadyVisited = true;
	}

	public bool IsAlreadyVisited() {
		return alreadyVisited;
	}

	public int GetLayoutIndex() {
		return alreadyVisited ? -1 : layoutIndex;
	}
}
