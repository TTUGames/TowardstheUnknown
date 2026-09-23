using System.Collections.Generic;
using UnityEngine;

public class GenericRoomPool
{
    private List<Room> rooms;

    public GenericRoomPool(IEnumerable<Room> rooms) {
        this.rooms = new List<Room>(rooms);
	}

    public RoomInfo GetRoom() {
        Room selectedRoom = rooms[Random.Range(0, rooms.Count)];
        int spawnLayoutCount = selectedRoom.GetComponentsInChildren<SpawnLayout>().Length;
        if (spawnLayoutCount == 0)
            throw new System.Exception(selectedRoom + " has no SpawnLayout");
        return new RoomInfo(selectedRoom, Random.Range(0, spawnLayoutCount));
	}
}
