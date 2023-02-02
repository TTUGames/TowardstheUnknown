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
        List<SpawnLayout> spawnLayouts = new List<SpawnLayout>(selectedRoom.GetComponentsInChildren<SpawnLayout>());
        List<int> onEnterSpawnLayoutIndexes = new List<int>();
        for (int i = 0; i < spawnLayouts.Count; ++i) {
            if (!spawnLayouts[i].IsRoomReward()) onEnterSpawnLayoutIndexes.Add(i);
        }
        if (useSpawnLayouts && onEnterSpawnLayoutIndexes.Count == 0)
            throw new System.Exception("This RoomPool is supposed to use SpawnLayouts but " + selectedRoom + " has none");
        return new RoomInfo(selectedRoom, useSpawnLayouts ? onEnterSpawnLayoutIndexes[Random.Range(0, onEnterSpawnLayoutIndexes.Count)] : -1);
	}
}
