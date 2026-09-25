using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The map of the HUD: the rooms around the visited ones are revealed, the current one highlighted.
/// The map can be set before the HUD is built: it is drawn once bound
/// </summary>
public class MinimapPanel
{
    private const int RoomSize = 30;

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
                // The grid is drawn rotated: its x axis goes down the map, from its center
                room.style.left = (mapSize.x - 1 - y) * RoomSize - RoomSize / 2f;
                room.style.top = (mapSize.x - 1 - x) * RoomSize - RoomSize / 2f;
                room.Add(new VisualElement { pickingMode = PickingMode.Ignore });
                grid.Add(room);
                rooms[x, y] = room;
            }
        Refresh();
    }

    private void Refresh()
    {
        if (rooms == null) return;
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
