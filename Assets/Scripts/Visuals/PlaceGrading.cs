using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Tells the descent through the colors: over the game's blue-grey grading, the cliffs open on a lighter, more neutral one,
/// and the rooms grow warmer and greener as they near Drareg's garden (the antechamber, by the rooms between). Two global
/// volumes made at the start blend in and out over a few seconds at each room entry; only their color filter and saturation
/// change, the filter keeping the image's luminance, so that the luminosity and contrast settings stay. Anniversary only: the Classic keeps its single grading
/// </summary>
public class PlaceGrading : MonoBehaviour
{
    [SerializeField, Tooltip("The color filter of the cliffs, over the open void")] private Color cliffFilter = new Color(0.88f, 0.9f, 0.95f);
    [SerializeField, Tooltip("The color filter next to the garden")] private Color gardenFilter = new Color(0.86f, 0.93f, 0.82f);
    [SerializeField, Range(-100, 100), Tooltip("Saturation added next to the garden")] private float gardenSaturation = 10;
    [SerializeField, Min(1), Tooltip("Rooms from the antechamber within which the garden's colors show, strongest beside it")] private int gardenReach = 3;
    [SerializeField, Min(0.01f), Tooltip("Real seconds to blend into a room's grading")] private float blendDuration = 2;
    [SerializeField, Tooltip("Above the game's volumes (0), under the antechamber's (1)")] private float priority = 0.5f;

    private Volume cliff, garden;
    private float cliffTarget, gardenTarget;

    private void Awake()
    {
        cliff = MakeVolume("Cliff grading", profile => profile.Add<ColorAdjustments>().colorFilter.Override(HueOnly(cliffFilter)));
        garden = MakeVolume("Garden grading", profile =>
        {
            ColorAdjustments adjustments = profile.Add<ColorAdjustments>();
            adjustments.colorFilter.Override(HueOnly(gardenFilter));
            adjustments.saturation.Override(gardenSaturation);
        });
    }

    // The filter scaled to a luminance of 1 (the filter is HDR): it tints the image without darkening it. The exposure
    // can't make up for it, it is the luminosity setting's
    private static Color HueOnly(Color filter)
    {
        float luminance = 0.2126f * filter.r + 0.7152f * filter.g + 0.0722f * filter.b;
        return luminance > 0 ? new Color(filter.r / luminance, filter.g / luminance, filter.b / luminance, 1) : Color.white;
    }

    private Volume MakeVolume(string name, System.Action<VolumeProfile> setup)
    {
        var holder = new GameObject(name);
        holder.transform.SetParent(transform, false);
        Volume volume = holder.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = priority;
        volume.weight = 0;
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        setup(profile);
        volume.sharedProfile = profile;
        return volume;
    }

    private void OnDestroy()
    {
        if (cliff != null) Destroy(cliff.sharedProfile);
        if (garden != null) Destroy(garden.sharedProfile);
    }

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
        Map map = GameScene.Map;
        if (map != null && map.CurrentRoom != null) OnRoomEntered(map.CurrentRoom, false);
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        cliffTarget = gardenTarget = 0;
        if (cliff != null) cliff.weight = 0;
        if (garden != null) garden.weight = 0;
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        cliffTarget = room.place == RoomPlace.CLIFF ? 1 : 0;
        int distance = RoomsToAntechamber();
        // The garden itself at full: the antechamber's own volume, above, then takes over
        gardenTarget = room.type is RoomType.ANTECHAMBER or RoomType.BOSS ? 1 : distance < 0 ? 0 : Mathf.Clamp01(1 - (float)distance / (gardenReach + 1));
        gardenTarget *= gardenTarget;
    }

    private void Update()
    {
        float step = Time.unscaledDeltaTime / blendDuration;
        if (cliff != null) cliff.weight = Mathf.MoveTowards(cliff.weight, cliffTarget, step);
        if (garden != null) garden.weight = Mathf.MoveTowards(garden.weight, gardenTarget, step);
    }

    /// <summary>
    /// Rooms between the current one and the antechamber, through the map's rooms; -1 if there is none
    /// </summary>
    private static int RoomsToAntechamber()
    {
        Map map = GameScene.Map;
        IReadOnlyList<IReadOnlyList<RoomInfo>> rooms = map.Rooms;
        Vector2Int start = map.CurrentRoomPosition;
        var distances = new Dictionary<Vector2Int, int> { [start] = 0 };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        Vector2Int[] steps = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            Vector2Int position = queue.Dequeue();
            RoomInfo info = rooms[position.x][position.y];
            if (info != null && info.GetRoomType() == RoomType.ANTECHAMBER) return distances[position];
            foreach (Vector2Int step in steps)
            {
                Vector2Int next = position + step;
                if (next.x < 0 || next.y < 0 || next.x >= rooms.Count || next.y >= rooms[next.x].Count || rooms[next.x][next.y] == null || distances.ContainsKey(next)) continue;
                distances[next] = distances[position] + 1;
                queue.Enqueue(next);
            }
        }
        return -1;
    }
}
