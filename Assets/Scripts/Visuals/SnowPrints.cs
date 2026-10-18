using UnityEngine;

/// <summary>
/// The prints the steps leave in the snow (<see cref="FootstepDust.Planted"/>): flat marks laid on the ground, darkening the
/// snow they press (a multiplied tint, which leaves the dark rock almost as it is), fading away over their lifetime. The
/// tiles' snow is a dusting of Snow Lit's noise, not a layer a shader could dig: the prints are decals of their own. None in
/// Drareg's garden, which has no snow (<see cref="GardenWeather.Sheltered"/>), and those of a room are cleared as the next is
/// entered, the rooms all standing at the same place. Anniversary only: the Classic leaves no print, as the original
/// </summary>
public class SnowPrints : MonoBehaviour
{
    [SerializeField, Tooltip("Lays the prints: world space, horizontal billboards, no emission of its own; made under this object")] private ParticleSystem printPrefab;
    [SerializeField, Tooltip("Whether the current room is sheltered from the snow")] private GardenWeather weather;
    [SerializeField, Tooltip("Height of the prints above the entity's ground, against z-fighting")] private float lift = 0.006f;
    [SerializeField, Range(0, 1), Tooltip("Width of a print, of its length")] private float width = 0.55f;

    private ParticleSystem prints;

    private void OnEnable()
    {
        FootstepDust.Planted += OnPlanted;
        GameEvents.RoomEntered += OnRoomEntered;
    }

    private void OnDisable()
    {
        FootstepDust.Planted -= OnPlanted;
        GameEvents.RoomEntered -= OnRoomEntered;
        if (prints != null) prints.Clear();
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (prints != null) prints.Clear();
    }

    private void OnPlanted(Vector3 position, float yaw, float size)
    {
        if (weather != null && weather.Sheltered) return;
        if (prints == null) prints = Instantiate(printPrefab, transform);
        position.y += lift;
        prints.Emit(new ParticleSystem.EmitParams
        {
            position = position,
            rotation = yaw,
            startSize3D = new Vector3(size * width, size, 1),
        }, 1);
    }
}
