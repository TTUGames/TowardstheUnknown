using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField, Tooltip("Shown on the exits of a cleared room (paired with the Classic's in ClassicSkin)")] private GameObject exitVFX;

    private List<List<RoomInfo>> rooms = new List<List<RoomInfo>>();
    private PlayerMove player;

    private Room currentRoom = null;
    private Vector2Int currentRoomPosition = Vector2Int.zero;
    // The open exit under the pointer, whose portal and destination on the minimap are highlighted
    private TransitionTile hoveredExit;

    /// <summary>
    /// The room the player is in, null while changing rooms
    /// </summary>
    public Room CurrentRoom => currentRoom;

    /// <summary>
    /// The rooms of the map by position, null where there is none
    /// </summary>
    public IReadOnlyList<IReadOnlyList<RoomInfo>> Rooms => rooms;

    /// <summary>
    /// The position of the room the player is in, or going to while changing rooms
    /// </summary>
    public Vector2Int CurrentRoomPosition => currentRoomPosition;

    /// <summary>
    /// A randomly generated run, the real game, rather than a test map (sandbox, gallery, showcase)
    /// </summary>
    public bool IsRandomRun { get; private set; }

    /// <summary>
    /// The seed of a random run's map, 0 for a test map
    /// </summary>
    public int Seed { get; private set; }

	private void Awake() {
        player = GameScene.Player.GetComponent<PlayerMove>();

        MapGeneration generation = GetComponent<MapGeneration>();
        IsRandomRun = generation is RandomMapGeneration;
        rooms = generation.Generate();
        if (generation is RandomMapGeneration random) Seed = random.Seed;
        currentRoomPosition = generation.GetSpawnPosition();
    }

	void Start()
    {
        StartCoroutine(EnterRoom(Direction.NULL));
    }

    private void OnEnable() {
        BoardPointer.TileHovered += OnTileHovered;
        BoardPointer.TileClicked += OnTileClicked;
        GameEvents.CombatStarted += ClearHoveredExit;
    }

    private void OnDisable() {
        BoardPointer.TileHovered -= OnTileHovered;
        BoardPointer.TileClicked -= OnTileClicked;
        GameEvents.CombatStarted -= ClearHoveredExit;
    }

    private void OnTileHovered(Tile tile) {
        TransitionTile exit = tile != null ? tile.GetComponent<TransitionTile>() : null;
        SetHoveredExit(exit != null && exit.IsOpen ? exit : null);
    }

    // An open exit clicked: the player sets off for it
    private void OnTileClicked(Tile tile) {
        if (tile.TryGetComponent(out TransitionTile exit) && exit.IsOpen) exit.Click();
    }

    private void ClearHoveredExit() => SetHoveredExit(null);

    private void SetHoveredExit(TransitionTile exit) {
        if (exit == hoveredExit) return;
        if (hoveredExit != null) hoveredExit.SetHovered(false);
        hoveredExit = exit;
        if (exit != null) exit.SetHovered(true);
        GameEvents.TargetExit(exit != null ? currentRoomPosition + DirectionConverter.DirToVect(exit.direction) : null);
    }

    /// <summary>
    /// Loads the room at the current position, deploys the player coming from <paramref name="fromDirection"/> and checks for combat
    /// </summary>
    /// <param name="fromDirection">The direction from which the player entered the room</param>
    private IEnumerator EnterRoom(Direction fromDirection) {
        Vector2Int pos = currentRoomPosition;
        currentRoom = rooms[pos.x][pos.y].LoadRoom(direction => RoomExists(pos + DirectionConverter.DirToVect(direction)), exitVFX);
        foreach (TransitionTile exit in currentRoom.Exits) {
            Vector2Int next = pos + DirectionConverter.DirToVect(exit.direction);
            exit.SetDestination(Destination(rooms[next.x][next.y]));
        }

        yield return currentRoom.GetComponent<PlayerDeploy>().DeployPlayer(player.transform, fromDirection);
        if (fromDirection != Direction.NULL) yield return GameScene.UI.Fade.Reveal();

        player.IsMapTransitioning = false;
        TurnSystem.Instance.CheckForCombatStart();
    }

    /// <summary>
    /// What an exit leading to <paramref name="room"/> shows, as the minimap: a relic lies there, visited, or unknown
    /// </summary>
    private static ExitDestination Destination(RoomInfo room) {
        if (room.HasLoot) return ExitDestination.TREASURE;
        return room.IsAlreadyVisited() ? ExitDestination.VISITED : ExitDestination.COMBAT;
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
    /// Deactivates the current room, kept for a next visit, and loads the one on the chosen side.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private IEnumerator MoveMapOnSide(Direction direction) {
        SetHoveredExit(null);
        currentRoom.enabled = false;
        GameEvents.LeaveRoom();

        yield return GameScene.UI.Fade.Cover();

        currentRoom.gameObject.SetActive(false);

        currentRoomPosition += DirectionConverter.DirToVect(direction);
        yield return EnterRoom(DirectionConverter.GetOppositeDirection(direction));
    }
}
