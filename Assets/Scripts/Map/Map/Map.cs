using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private List<List<RoomInfo>> rooms = new List<List<RoomInfo>>();
    private GameObject player;
    private GameObject ui;

    private Room currentRoom = null;
    private Vector2Int currentRoomPosition = Vector2Int.zero;

	private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        ui = GameObject.FindGameObjectWithTag("UI");

        Debug.Log("GENERATING");
        rooms.Add(new List<RoomInfo>());
        rooms[0].Add(new RoomInfo(Resources.Load<Room>("Prefabs/Maps/Map2_Codir2"), 0));
        rooms[0].Add(new RoomInfo(Resources.Load<Room>("Prefabs/Maps/Map2_Codir2"), 1));
    }

	// Start is called before the first frame update
	void Start()
    {
        LoadRoom(currentRoomPosition);
    }

    private void LoadRoom(Vector2Int pos) {
        currentRoom = rooms[pos.x][pos.y].LoadRoom(RoomExists(pos + Vector2Int.up), RoomExists(pos + Vector2Int.down), RoomExists(pos + Vector2Int.left), RoomExists(pos + Vector2Int.right));
    }

    private bool RoomExists(Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0) return false;
        return rooms.Count > pos.x && rooms[pos.x].Count > pos.y && rooms[pos.x][pos.y] != null;
	}

    public void MoveToAdjacentRoom(Direction direction) {
        StartCoroutine(MoveMapOnSide(direction));
    }

    /// <summary>
    /// Create the new map and slide it at the position of the current map. <br/>
    /// </summary>
    /// <param name="direction">This is the number of the direction where the exit has been triggered</param>
    /// <returns></returns>
    private IEnumerator MoveMapOnSide(Direction direction) {
        currentRoom.enabled = false;

        yield return ui.GetComponent<UIFade>().FadeEnum(true);

        Destroy(currentRoom.gameObject);
        yield return new WaitForEndOfFrame();

        currentRoomPosition += DirectionConverter.DirToVect(direction);
        LoadRoom(currentRoomPosition);

        yield return ui.GetComponent<UIFade>().FadeEnum(false);


        player.GetComponent<PlayerMove>().isMapTransitioning = false;

    }
}
