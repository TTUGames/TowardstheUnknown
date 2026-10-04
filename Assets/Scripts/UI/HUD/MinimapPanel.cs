using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The map of the HUD: the rooms around the visited ones are revealed, the current one highlighted and centered
/// (<see cref="EditionProfile.minimapCentered"/>; otherwise the rooms keep the original's fixed grid). A room where a
/// relic lies shows it (<see cref="RoomInfo.HasLoot"/>, again on <see cref="GameEvents.LootChanged"/>), and the room an
/// exit under the pointer leads to is marked (<see cref="GameEvents.ExitTargeted"/>). It follows the map's rooms and the
/// current one on <see cref="GameEvents.RoomEntered"/>, reading them from <see cref="Map"/>, and at once when bound after
/// </summary>
public class MinimapPanel
{
    private const int RoomSize = 26;
    // The original's grid: 30 point rooms, touching, from the top left of the view
    private const int OriginalRoomSize = 30;

    private VisualElement root;
    private VisualElement[,] rooms;
    private IReadOnlyList<IReadOnlyList<RoomInfo>> roomInfos;
    private Vector2Int currentRoom;
    private readonly HashSet<Vector2Int> revealed = new();
    private readonly HashSet<Vector2Int> visited = new();
    private bool hidden;
    private Vector2Int? targetRoom;
    // Beats the targeted room, toggling its targeted--beat class
    private IVisualElementScheduledItem beat;
    private const long BeatMilliseconds = 420;

    public void Bind(VisualElement root)
    {
        this.root = root;
        Unbind();
        GameEvents.LootChanged += OnLootChanged;
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.ExitTargeted += SetTargetRoom;
        Build();
        if (GameScene.Map != null && GameScene.Map.CurrentRoom != null) FollowMap();
    }

    /// <summary>
    /// Stops following the game's events, once the HUD is destroyed
    /// </summary>
    public void Unbind()
    {
        GameEvents.LootChanged -= OnLootChanged;
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.ExitTargeted -= SetTargetRoom;
    }

    private void OnLootChanged(Room room) => Refresh();

    private void OnRoomEntered(Room room, bool firstVisit) => FollowMap();

    // A new map (a new run) is drawn again, then the current room taken
    private void FollowMap()
    {
        Map map = GameScene.Map;
        if (!ReferenceEquals(roomInfos, map.Rooms)) SetMap(map.Rooms);
        SetCurrentRoom(map.CurrentRoomPosition);
    }

    /// <summary>
    /// Marks the room the player would go to, null for none
    /// </summary>
    private void SetTargetRoom(Vector2Int? position)
    {
        targetRoom = position;
        Refresh();
        if (root == null) return;
        beat ??= root.schedule.Execute(Beat).Every(BeatMilliseconds);
        if (position != null) beat.Resume();
        else beat.Pause();
    }

    private void Beat()
    {
        if (rooms == null || targetRoom is not Vector2Int target) return;
        VisualElement room = rooms[target.x, target.y];
        room?.ToggleInClassList("targeted--beat");
    }

    private void SetMap(IReadOnlyList<IReadOnlyList<RoomInfo>> roomInfos)
    {
        this.roomInfos = roomInfos;
        revealed.Clear();
        visited.Clear();
        targetRoom = null;
        Build();
    }

    private void SetCurrentRoom(Vector2Int position)
    {
        currentRoom = position;
        visited.Add(position);
        revealed.Add(position);
        foreach (Vector2Int direction in new[] { Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right })
            revealed.Add(position + direction);
        Refresh();
    }

    /// <summary>
    /// Shows every room of the map, as revealed (a debug shortcut, DevCheats)
    /// </summary>
    public void RevealAll()
    {
        if (roomInfos == null) return;
        for (int x = 0; x < roomInfos.Count; x++)
            for (int y = 0; y < roomInfos[x].Count; y++)
                if (roomInfos[x][y] != null) revealed.Add(new Vector2Int(x, y));
        Refresh();
    }

    /// <summary>
    /// Hides the map while a menu covers the game
    /// </summary>
    public void SetVisible(bool visible)
    {
        hidden = !visible;
        root?.EnableInClassList("hidden", hidden);
    }

    /// <summary>
    /// Draws the rooms again, where the edition places them
    /// </summary>
    public void Redraw() => Build();

    private void Build()
    {
        if (root == null) return;
        root.EnableInClassList("hidden", hidden);
        VisualElement grid = root.Q("MinimapRooms");
        grid.Clear();
        if (roomInfos == null) return;

        Vector2Int mapSize = new Vector2Int(roomInfos.Count, roomInfos[0].Count);
        rooms = new VisualElement[mapSize.x, mapSize.y];
        for (int x = 0; x < mapSize.x; x++)
            for (int y = 0; y < mapSize.y; y++)
            {
                if (roomInfos[x][y] == null) continue;
                var room = new VisualElement { pickingMode = PickingMode.Ignore };
                room.AddToClassList("minimap-room");
                room.AddToClassList("minimap-room--" + roomInfos[x][y].GetRoomType().ToString().ToLowerInvariant());
                Vector2 position = DisplayPosition(new Vector2Int(x, y));
                float size = Edition.Profile.minimapCentered ? RoomSize : OriginalRoomSize;
                room.style.left = position.x - size / 2;
                room.style.top = position.y - size / 2;
                // The type's icon (boss, antechamber), and the relic lying there
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList("minimap-room__icon");
                room.Add(icon);
                var loot = new VisualElement { pickingMode = PickingMode.Ignore };
                loot.AddToClassList("minimap-room__loot");
                room.Add(loot);
                grid.Add(room);
                rooms[x, y] = room;
            }
        Refresh();
    }

    // The grid is drawn rotated: its x axis goes up the map. Centered, around the first room; otherwise the room's center
    // from the view's top left, as the original's (which took the map's first size for both axes)
    private Vector2 DisplayPosition(Vector2Int room)
    {
        if (Edition.Profile.minimapCentered) return new Vector2(-room.y, -room.x) * RoomSize;
        int last = roomInfos.Count - 1;
        return new Vector2(last - room.y, last - room.x) * OriginalRoomSize + Vector2.one * (OriginalRoomSize / 2f);
    }

    private void Refresh()
    {
        if (rooms == null) return;
        // The rooms slide to keep the current one at the center of the map (transition of Hud.uss)
        Vector2 center = Edition.Profile.minimapCentered ? DisplayPosition(currentRoom) : Vector2.zero;
        root.Q("MinimapRooms").style.translate = new Translate(-center.x, -center.y);
        for (int x = 0; x < rooms.GetLength(0); x++)
            for (int y = 0; y < rooms.GetLength(1); y++)
            {
                VisualElement room = rooms[x, y];
                if (room == null) continue;
                var position = new Vector2Int(x, y);
                room.EnableInClassList("revealed", revealed.Contains(position));
                room.EnableInClassList("visited", visited.Contains(position));
                room.EnableInClassList("current", position == currentRoom && visited.Contains(position));
                room.EnableInClassList("loot", roomInfos[x][y].HasLoot);
                room.EnableInClassList("targeted", position == targetRoom);
                if (position != targetRoom) room.RemoveFromClassList("targeted--beat");
            }
    }
}
