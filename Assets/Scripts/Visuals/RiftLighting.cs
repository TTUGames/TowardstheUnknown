using UnityEngine;

/// <summary>
/// The light of the rift: when a room is entered, its main light (the brightest spot) shines through the rift's cracks
/// (a cookie), and a volume around the room makes that light and the other lights visible in the air, over a mist
/// lying below the tiles (Rift Volumetrics shader)
/// </summary>
public class RiftLighting : MonoBehaviour
{
    [SerializeField, Tooltip("The cracks of the rift, projected by the main light")] private Texture cookie;
    [SerializeField, Tooltip("A box drawn with the Rift Volumetrics material")] private MeshRenderer volume;
    [SerializeField, Tooltip("Around the room's meshes, in meters")] private float margin = 1;
    [SerializeField, Tooltip("How deep the volume goes below the room, for the mist, in meters")] private float depthBelow = 8;
    [SerializeField, Tooltip("The main light shines only through the cracks: it is made this much stronger so that they stand out")] private float mainLightBoost = 1.35f;
    [SerializeField, Tooltip("Air left out right under the main light, in meters")] private float clearUnderLight = 4;
    [SerializeField, Tooltip("The main light leans this much, in degrees, in a direction of its own in each room: the rift turns and the rays slant")] private Vector2 tilt = new Vector2(4, 10);

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
    }

    private void OnRoomEntered(Room room, bool firstVisit) => Apply(room.gameObject);

    public void Apply(GameObject room)
    {
        Light main = null;
        foreach (Light light in room.GetComponentsInChildren<Light>())
            if (light.type == LightType.Spot && (main == null || light.intensity > main.intensity)) main = light;
        // Once per room: a room left then entered again keeps its light
        if (main != null && cookie != null && main.cookie != cookie)
        {
            main.cookie = cookie;
            main.intensity *= mainLightBoost;
            var random = new System.Random(StableHash(room.name));
            float azimuth = (float)random.NextDouble() * 360;
            float lean = Mathf.Lerp(tilt.x, tilt.y, (float)random.NextDouble());
            main.transform.rotation = Quaternion.AngleAxis(azimuth, Vector3.up) * Quaternion.AngleAxis(lean, Vector3.right) * Quaternion.LookRotation(Vector3.down, Vector3.forward);
        }

        if (volume == null) return;
        Bounds bounds = new Bounds();
        bool any = false;
        foreach (Renderer renderer in room.GetComponentsInChildren<MeshRenderer>())
        {
            if (!renderer.enabled) continue;
            if (!any) bounds = renderer.bounds;
            else bounds.Encapsulate(renderer.bounds);
            any = true;
        }
        if (!any) return;
        // Right under the light, the air would glare: the rays start a little lower
        float top = main != null ? main.transform.position.y - clearUnderLight : bounds.max.y;
        float bottom = bounds.min.y - depthBelow;
        Vector3 size = new Vector3(bounds.size.x + margin * 2, top - bottom, bounds.size.z + margin * 2);
        volume.transform.SetPositionAndRotation(new Vector3(bounds.center.x, (top + bottom) / 2, bounds.center.z), Quaternion.identity);
        volume.transform.localScale = size;
    }

    // string.GetHashCode may change between runs
    private static int StableHash(string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text) hash = hash * 31 + c;
            return hash & 0x7fffffff;
        }
    }
}
