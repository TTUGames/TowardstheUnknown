using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericRoomPool : MonoBehaviour
{
    private List<Room> rooms;
    bool useSpawnLayouts;

    public GenericRoomPool(string folderPath, bool useSpawnLayouts = false) {
        rooms = new List<Room> (Resources.LoadAll<Room>(folderPath));
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
