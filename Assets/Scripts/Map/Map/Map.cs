using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private List<List<Room>> rooms = new List<List<Room>>();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GENERATING");
        rooms.Add(new List<Room>());
        rooms[0].Add(Resources.Load<Room>("Prefabs/Maps/Map2_Codir2"));

        LoadRoom(0, 0);
    }

    private void LoadRoom(int x, int y) {
        Instantiate<Room>(rooms[x][y]);
	}

}
