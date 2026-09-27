using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The map of the HUD: the rooms around the visited ones are revealed, the current one highlighted and centered
/// (<see cref="EditionProfile.minimapCentered"/>; otherwise the rooms keep the original's fixed grid).
/// The map can be set before the HUD is built: it is drawn once bound
/// </summary>
public class MinimapPanel
{
    private const int RoomSize = 26;
    // The original's grid: 30 point rooms, touching, from the top left of the view
    private const int OriginalRoomSize = 30;

    private VisualElement root;
    private VisualElement[,] rooms;
    private List<List<RoomInfo>> roomInfos;
    private Vector2Int currentRoom;
    private readonly HashSet<Vector2Int> revealed = new();
    private readonly HashSet<Vector2Int> visited = new();
    private bool hidden;

    public void Bind(VisualElement root)
    {
        this.root = root;
        Build();
    }

    public void SetMap(List<List<RoomInfo>> roomInfos)
    {
        this.roomInfos = roomInfos;
        revealed.Clear();
        visited.Clear();
        Build();
    }

    public void SetCurrentRoom(Vector2Int position)
    {
        currentRoom = position;
        visited.Add(position);
        revealed.Add(position);
        foreach (Vector2Int direction in new[] { Vector2Int.down, Vector2Int.left, Vector2Int.up, Vector2Int.right })
            revealed.Add(position + direction);
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
                room.Add(new VisualElement { pickingMode = PickingMode.Ignore });
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
            }
    }
}
