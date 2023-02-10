using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapGeneration))]
public class Map : MonoBehaviour
{
    private List<List<RoomInfo>> rooms = new List<List<RoomInfo>>();
    private GameObject player;
    private GameObject ui;
    private Minimap minimap;

    [SerializeField] Vector2Int size;
    [SerializeField] Vector2Int spawnPosition;

    private Room currentRoom = null;
    private Vector2Int currentRoomPosition = Vector2Int.zero;

    public Room CurrentRoom { get => currentRoom; }

	private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        ui = GameObject.FindGameObjectWithTag("UI");
        minimap = FindObjectOfType<Minimap>();

        Debug.Log("GENERATING");
        rooms = GetComponent<MapGeneration>().Generate();
        minimap.SetMap(rooms);
        /*RoomPool roomPool = new RoomPool("Prefabs/Rooms/CombatRooms");
        //Room treasureRoomPrefab = Resources.Load<Room>("Prefabs/Rooms/TreasureRooms/TreasureRoom1");
        for (int x = 0; x < size.x; ++x) {
            rooms.Add(new List<RoomInfo>());
            for(int y = 0; y < size.y; ++y) {
                rooms[x].Add(roomPool.GetRoom(1));
                //rooms[x].Add(new RoomInfo(treasureRoomPrefab, 0));
			}
		}
        rooms[spawnPosition.x][spawnPosition.y] = new RoomInfo(Resources.Load<Room>("Prefabs/Rooms/SpawnRooms/SpawnRoom1"), -1);*/
        currentRoomPosition = GetComponent<MapGeneration>().GetSpawnPosition();
    }

	// Start is called before the first frame update
	void Start()
    {
        StartCoroutine(LoadFirstRoom());
    }

    /// <summary>
    /// Loads the first room. 
    /// TODO : remove a probable crash if the room does not contain combat and does not have a north exit
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadFirstRoom() {
        LoadRoom(currentRoomPosition);
        yield return DeployPlayer(Direction.NULL);
        player.GetComponent<PlayerMove>().isMapTransitioning = false;
        FindObjectOfType<TurnSystem>().CheckForCombatStart();
    }

    /// <summary>
    /// Loads the room at <paramref name="pos"/> and deploys the player
    /// </summary>
    /// <param name="pos">The room's position in the map</param>
    /// <param name="fromDirection">The direction from which the player entered the room</param>
    /// <returns></returns>
    private void LoadRoom(Vector2Int pos) {
        currentRoom = rooms[pos.x][pos.y].LoadRoom(RoomExists(pos + Vector2Int.up), RoomExists(pos + Vector2Int.down), RoomExists(pos + Vector2Int.left), RoomExists(pos + Vector2Int.right));
        minimap.SetCurrentRoom(pos);
    }

    private IEnumerator DeployPlayer(Direction fromDirection) {
        yield return currentRoom.GetComponent<PlayerDeploy>().DeployPlayer(FindObjectOfType<PlayerTurn>().transform, fromDirection);
    }

    /// <summary>
    /// Checks if the room at <paramref name="pos"/> exists
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool RoomExists(Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0) return false;
        return rooms.Count > pos.x && rooms[pos.x].Count > pos.y && rooms[pos.x][pos.y] != null;
	}

    /// <summary>
    /// Moves the player to the adjacent room in the given direction
    /// </summary>
    /// <param name="direction"></param>
    public void MoveToAdjacentRoom(Direction direction) {
        StartCoroutine(MoveMapOnSide(direction));
    }

    /// <summary>
    /// Destroys the current room and loads the one on the chosen side.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private IEnumerator MoveMapOnSide(Direction direction) {
        currentRoom.enabled = false;

        yield return ui.GetComponent<UIFade>().FadeIn();

        Destroy(currentRoom.gameObject);
        yield return new WaitForEndOfFrame();

        currentRoomPosition += DirectionConverter.DirToVect(direction);
        LoadRoom(currentRoomPosition);

        yield return DeployPlayer(DirectionConverter.GetOppositeDirection(direction));
        yield return ui.GetComponent<UIFade>().FadeOut();

        player.GetComponent<PlayerMove>().isMapTransitioning = false;
        FindObjectOfType<TurnSystem>().CheckForCombatStart();
    }
}
