using System.Collections.Generic;
using UnityEngine;

public class GenericRoomPool
{
    private List<Room> rooms;
    bool useSpawnLayouts;

    public GenericRoomPool(IEnumerable<Room> rooms, bool useSpawnLayouts = false) {
        this.rooms = new List<Room>(rooms);
        this.useSpawnLayouts = useSpawnLayouts;
	}

    public RoomInfo GetRoom() {
        Room selectedRoom = rooms[Random.Range(0, rooms.Count)];
        int spawnLayoutCount = selectedRoom.GetComponentsInChildren<SpawnLayout>().Length;
        if (useSpawnLayouts && spawnLayoutCount == 0)
            throw new System.Exception("This RoomPool is supposed to use SpawnLayouts but " + selectedRoom + " has none");
        return new RoomInfo(selectedRoom, useSpawnLayouts ? Random.Range(0, spawnLayoutCount) : -1);
	}
}
