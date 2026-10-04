using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Probes of the room-inspect skill, compiled in memory by run_script against the live game (Play mode): list the map's rooms,
// jump straight to one, find objects in the current room, and render close-ups of them from the game camera's angle.
public static class Rooms
{
    static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    static List<List<RoomInfo>> Grid(Map map) => (List<List<RoomInfo>>)typeof(Map).GetField("rooms", Private).GetValue(map);

    static string PrefabName(RoomInfo info) => ((Room)typeof(RoomInfo).GetField("roomPrefab", Private).GetValue(info)).name;

    /// <summary>
    /// Every room of the map: its cell, prefab and type, the current one marked with *
    /// </summary>
    public static string List()
    {
        Map map = GameScene.Map;
        var grid = Grid(map);
        var current = (Vector2Int)typeof(Map).GetField("currentRoomPosition", Private).GetValue(map);
        var lines = new List<string>();
        for (int x = 0; x < grid.Count; x++)
            for (int y = 0; y < grid[x].Count; y++)
                if (grid[x][y] != null)
                    lines.Add($"{(current == new Vector2Int(x, y) ? "*" : " ")} ({x},{y}) {PrefabName(grid[x][y])} {grid[x][y].GetRoomType()}");
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Jumps to the room whose prefab is named <paramref name="name"/> (exact, else the first containing it), through the
    /// usual transition: the map stands next to it, then moves in. Wait ~8 s, then check with Rooms.Current
    /// </summary>
    public static string Go(string name)
    {
        Map map = GameScene.Map;
        if (map.CurrentRoom == null) return "busy: the map is changing rooms";
        var grid = Grid(map);
        Vector2Int? target = null;
        foreach (bool exact in new[] { true, false })
        {
            for (int x = 0; x < grid.Count && target == null; x++)
                for (int y = 0; y < grid[x].Count && target == null; y++)
                {
                    if (grid[x][y] == null) continue;
                    string prefab = PrefabName(grid[x][y]);
                    if (exact ? prefab == name : prefab.Contains(name)) target = new Vector2Int(x, y);
                }
            if (target != null) break;
        }
        if (target == null) return $"no room named {name} in this map (Rooms.List)";
        // Stand on a neighbor it has an exit to, so that the player deploys at that exit (west of it if none, a cell that need
        // not exist), and move into it
        Direction way = Direction.EAST;
        foreach (Direction direction in new[] { Direction.EAST, Direction.NORTH, Direction.SOUTH, Direction.WEST })
        {
            Vector2Int from = target.Value - DirectionConverter.DirToVect(direction);
            if (from.x < 0 || from.y < 0 || from.x >= grid.Count || from.y >= grid[from.x].Count || grid[from.x][from.y] == null) continue;
            way = direction;
            break;
        }
        typeof(Map).GetField("currentRoomPosition", Private).SetValue(map, target.Value - DirectionConverter.DirToVect(way));
        map.MoveToAdjacentRoom(way);
        return $"going to {PrefabName(grid[target.Value.x][target.Value.y])} at {target.Value}";
    }

    /// <summary>
    /// The current room's name, or "changing" during a transition
    /// </summary>
    public static string Current() => GameScene.Map.CurrentRoom != null ? GameScene.Map.CurrentRoom.name : "changing";

    /// <summary>
    /// The objects of the current room whose name contains <paramref name="filter"/> (case-insensitive), with their path,
    /// world position, rotation, scale, active state and components: to analyse a room's objects quickly
    /// </summary>
    public static string Find(string filter)
    {
        Room room = GameScene.Map.CurrentRoom;
        if (room == null) return "changing";
        var lines = new List<string>();
        foreach (Transform t in room.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
            string components = string.Join(",", t.GetComponents<Component>().Where(c => c != null && c is not Transform).Select(c => c.GetType().Name));
            lines.Add($"{PathOf(t, room.transform)} active={t.gameObject.activeInHierarchy} pos={t.position.ToString("0.000")} rot={t.eulerAngles.ToString("0.0")} lossy={t.lossyScale.ToString("0.000")} [{components}]");
            if (lines.Count == 60) { lines.Add("…"); break; }
        }
        return lines.Count == 0 ? $"nothing named *{filter}* in {room.name}" : string.Join("\n", lines);
    }

    /// <summary>
    /// Renders close-ups of the current room's active objects whose name contains <paramref name="filter"/>, from the game
    /// camera's angle and with its post-processing, <paramref name="size"/> meters of half-height around each (orthographic),
    /// into <paramref name="folder"/>/<paramref name="label"/>-<index>.png, centered on what each shows, what stands in front clipped; <paramref name="only"/> -1 for all, else one index.
    /// A filter "*" frames the whole room
    /// </summary>
    public static string Shot(string folder, string label, string filter, float size, int only)
    {
        Room room = GameScene.Map.CurrentRoom;
        if (room == null) return "changing";
        Directory.CreateDirectory(folder);
        var targets = new List<(string name, Vector3 position)>();
        if (filter == "*")
        {
            Bounds bounds = new Bounds(room.transform.position, Vector3.zero);
            foreach (Renderer r in room.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            targets.Add((room.name, bounds.center));
            if (size <= 0) size = Mathf.Max(bounds.extents.x, bounds.extents.z) * 0.8f;
        }
        else
            foreach (Transform t in room.GetComponentsInChildren<Transform>())
                if (t.name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    targets.Add((PathOf(t, room.transform), Center(t)));
        if (targets.Count == 0) return $"nothing named *{filter}* in {room.name}";

        Camera main = Camera.main;
        var go = new GameObject("RoomInspectCamera");
        var lines = new List<string>();
        try
        {
            Camera cam = go.AddComponent<Camera>();
            cam.CopyFrom(main);
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            var mainData = main.GetComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = mainData == null || mainData.renderPostProcessing;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            if (mainData != null) data.volumeLayerMask = mainData.volumeLayerMask;
            const int w = 900, h = 900;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            for (int i = 0; i < targets.Count; i++)
            {
                if (only >= 0 && i != only) continue;
                cam.orthographic = true;
                cam.orthographicSize = size;
                cam.transform.rotation = main.transform.rotation;
                cam.transform.position = targets[i].position - main.transform.forward * 60f;
                // What stands between the camera and the target (a rock in front) is clipped
                cam.nearClipPlane = 60f - Mathf.Max(size * 2f, 2f);
                cam.farClipPlane = 300f;
                cam.Render();
                var previous = RenderTexture.active;
                RenderTexture.active = rt;
                var texture = new Texture2D(w, h, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                texture.Apply();
                RenderTexture.active = previous;
                string path = Path.Combine(folder, $"{label}-{i}.png").Replace('\\', '/');
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                lines.Add($"{i} {targets[i].name} at {targets[i].position.ToString("0.00")} -> {path}");
                if (lines.Count == 30) break;
            }
            cam.targetTexture = null;
            rt.Release();
        }
        finally { Object.DestroyImmediate(go); }
        return string.Join("\n", lines);
    }

    // The middle of what the object shows (its active renderers), else its pivot
    static Vector3 Center(Transform t)
    {
        var renderers = t.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r is not ParticleSystemRenderer).ToArray();
        if (renderers.Length == 0) return t.position;
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        return bounds.center;
    }

    static string PathOf(Transform t, Transform root)
    {
        var parts = new List<string>();
        for (; t != null && t != root; t = t.parent) parts.Insert(0, t.name);
        return string.Join("/", parts);
    }
}
