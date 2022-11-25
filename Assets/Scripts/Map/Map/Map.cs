using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private List<List<Room>> rooms = new List<List<Room>>();

    private Room currentRoom = null;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GENERATING");
        rooms.Add(new List<Room>());
        rooms[0].Add(Resources.Load<Room>("Prefabs/Maps/Map2_Codir2"));
        rooms[0].Add(Resources.Load<Room>("Prefabs/Maps/Map2_Codir2"));

        LoadRoom(Vector2Int.zero);
    }

    private void LoadRoom(Vector2Int pos) {
        currentRoom = Instantiate<Room>(rooms[pos.x][pos.y]);
        currentRoom.SetExits(RoomExists(pos + Vector2Int.up), RoomExists(pos + Vector2Int.down), RoomExists(pos + Vector2Int.left), RoomExists(pos + Vector2Int.right));
	}

    private bool RoomExists(Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0) return false;
        return rooms.Count > pos.x && rooms[pos.x].Count > pos.y && rooms[pos.x][pos.y] != null;
	}
}
