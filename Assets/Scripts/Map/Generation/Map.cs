using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField, Tooltip("Shown on the exits of a cleared room")] private GameObject exitVFX;

    private List<List<RoomInfo>> rooms = new List<List<RoomInfo>>();
    private PlayerMove player;
    private ScreenFade uiFade;
    private MinimapPanel minimap;

    private Room currentRoom = null;
    private Vector2Int currentRoomPosition = Vector2Int.zero;

	private void Awake() {
        player = GameScene.Player.GetComponent<PlayerMove>();
        uiFade = GameScene.UI.Fade;
        minimap = GameScene.UI.Minimap;

        MapGeneration generation = GetComponent<MapGeneration>();
        rooms = generation.Generate();
        minimap.SetMap(rooms);
        currentRoomPosition = generation.GetSpawnPosition();
    }

	void Start()
    {
        StartCoroutine(EnterRoom(Direction.NULL));
    }

    /// <summary>
    /// Loads the room at the current position, deploys the player coming from <paramref name="fromDirection"/> and checks for combat
    /// </summary>
    /// <param name="fromDirection">The direction from which the player entered the room</param>
    private IEnumerator EnterRoom(Direction fromDirection) {
        Vector2Int pos = currentRoomPosition;
        currentRoom = rooms[pos.x][pos.y].LoadRoom(direction => RoomExists(pos + DirectionConverter.DirToVect(direction)), exitVFX);
        minimap.SetCurrentRoom(pos);

        yield return currentRoom.GetComponent<PlayerDeploy>().DeployPlayer(player.transform, fromDirection);
        if (fromDirection != Direction.NULL) yield return uiFade.FadeOut();

        player.isMapTransitioning = false;
        TurnSystem.Instance.CheckForCombatStart();
    }

    /// <summary>
    /// Checks if the room at <paramref name="pos"/> exists
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool RoomExists(Vector2Int pos) {
        return pos.x >= 0 && pos.y >= 0 && rooms.Count > pos.x && rooms[pos.x].Count > pos.y && rooms[pos.x][pos.y] != null;
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
        GameEvents.LeaveRoom();

        yield return uiFade.FadeIn();

        Destroy(currentRoom.gameObject);
        yield return new WaitForEndOfFrame();

        currentRoomPosition += DirectionConverter.DirToVect(direction);
        yield return EnterRoom(DirectionConverter.GetOppositeDirection(direction));
    }
}
