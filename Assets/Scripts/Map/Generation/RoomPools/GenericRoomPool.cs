using System.Collections.Generic;
using UnityEngine;

public class GenericRoomPool
{
    private List<Room> rooms;

    public GenericRoomPool(IEnumerable<Room> rooms) {
        this.rooms = new List<Room>(rooms);
	}

    public RoomInfo GetRoom() {
        Room selectedRoom = GameRandom.Map.Pick(rooms);
        int layoutCount = selectedRoom.LayoutCount;
        if (layoutCount == 0)
            throw new System.Exception(selectedRoom + " has neither an enemy layout nor a treasure layout");
        return new RoomInfo(selectedRoom, GameRandom.Map.Range(0, layoutCount));
	}
}
