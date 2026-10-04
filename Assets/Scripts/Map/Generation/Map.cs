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
    // The side the first room is entered from: none at a run's start, the saved one for a suspended run
    private Direction startDirection = Direction.NULL;
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
        if (IsRandomRun && RunSave.Resumed != null) Resume(RunSave.Resumed);
    }

	void Start()
    {
        StartCoroutine(EnterRoom(startDirection));
    }

    /// <summary>
    /// A suspended run: its visited rooms as left, the player entering the saved room from its side
    /// </summary>
    private void Resume(RunSave.Data save) {
        foreach (Vector2Int visited in save.visitedRooms)
            if (RoomExists(visited)) rooms[visited.x][visited.y].MarkVisited();
        if (!RoomExists(save.room)) return;
        currentRoomPosition = save.room;
        startDirection = save.enteredFrom;
    }

    /// <summary>
    /// Saves the run as the player enters the current room from <paramref name="fromDirection"/>, before it loads
    /// (<see cref="RunSave"/>): continuing it enters this room again
    /// </summary>
    private void SaveRun(Direction fromDirection) {
        var save = new RunSave.Data {
            seed = Seed,
            room = currentRoomPosition,
            enteredFrom = fromDirection,
            health = GameScene.Player.Stats.CurrentHealth,
        };
        for (int x = 0; x < rooms.Count; x++)
            for (int y = 0; y < rooms[x].Count; y++)
                if (rooms[x][y] != null && rooms[x][y].IsAlreadyVisited()) save.visitedRooms.Add(new Vector2Int(x, y));
        foreach (TetrisInventoryItem item in GameScene.Player.Inventory.Data.Items)
            save.items.Add(new RunSave.Item { artifact = item.itemData.ID, slot = item.slot, rotation = item.rotation });
        GameScene.Run.Save(save);
        RunSave.Write(save);
    }

    // Inactive: the rooms made ahead under it sleep until they load
    private Transform preloadHolder;

    /// <summary>
    /// Makes the unvisited rooms next to the current one ahead, one per frame, so that walking into them costs only their
    /// waking: a room's instantiation is the bulk of its loading
    /// </summary>
    private IEnumerator PreloadNeighbors() {
        if (preloadHolder == null) {
            preloadHolder = new GameObject("Preloaded rooms").transform;
            preloadHolder.SetParent(transform, false);
            preloadHolder.gameObject.SetActive(false);
        }
        Vector2Int from = currentRoomPosition;
        foreach (Direction direction in new[] { Direction.NORTH, Direction.SOUTH, Direction.EAST, Direction.WEST }) {
            yield return null;
            //The player left meanwhile
            if (currentRoomPosition != from) yield break;
            Vector2Int next = from + DirectionConverter.DirToVect(direction);
            if (RoomExists(next)) rooms[next.x][next.y].Preload(preloadHolder);
        }
    }

    private void OnEnable() {
        GameEvents.RunEnded += OnRunEnded;
        BoardPointer.TileHovered += OnTileHovered;
        BoardPointer.TileClicked += OnTileClicked;
        GameEvents.CombatStarted += ClearHoveredExit;
    }

    private void OnDisable() {
        GameEvents.RunEnded -= OnRunEnded;
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

    // A run won or lost can't be continued
    private void OnRunEnded(bool isVictory) {
        if (IsRandomRun) RunSave.Delete();
    }

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
        // Between two rooms: not at the run's start, nor in a test map
        if (IsRandomRun && fromDirection != Direction.NULL && Edition.Profile.suspendRun) SaveRun(fromDirection);
        currentRoom = rooms[pos.x][pos.y].LoadRoom(direction => RoomExists(pos + DirectionConverter.DirToVect(direction)), exitVFX);
        foreach (TransitionTile exit in currentRoom.Exits) {
            Vector2Int next = pos + DirectionConverter.DirToVect(exit.direction);
            exit.SetDestination(Destination(rooms[next.x][next.y]));
        }

        yield return currentRoom.GetComponent<PlayerDeploy>().DeployPlayer(player.transform, fromDirection);
        if (fromDirection != Direction.NULL) yield return GameScene.UI.Fade.Reveal();

        player.IsMapTransitioning = false;
        TurnSystem.Instance.CheckForCombatStart();
        StartCoroutine(PreloadNeighbors());
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
    /// Goes straight to a room, entering it from a neighbor it has an exit to (a debug shortcut, DevCheats)
    /// </summary>
    public void JumpTo(Vector2Int room) {
        if (!RoomExists(room) || currentRoom == null) return;
        Direction way = Direction.EAST;
        foreach (Direction direction in new[] { Direction.EAST, Direction.NORTH, Direction.SOUTH, Direction.WEST }) {
            if (!RoomExists(room - DirectionConverter.DirToVect(direction))) continue;
            way = direction;
            break;
        }
        currentRoomPosition = room - DirectionConverter.DirToVect(way);
        MoveToAdjacentRoom(way);
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
