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
        int layoutCount = selectedRoom.LayoutCount;
        if (layoutCount == 0)
            throw new System.Exception(selectedRoom + " has neither an enemy layout nor a SpawnLayout");
        return new RoomInfo(selectedRoom, Random.Range(0, layoutCount));
	}
}
