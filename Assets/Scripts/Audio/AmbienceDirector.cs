using UnityEngine;

/// <summary>
/// Plays the ambience loop of the place the current room lies in (<see cref="Room.place"/>, Drareg's garden for the antechamber and
/// the boss room), and the water layer over it in the rooms with pools. In an edition without <see cref="EditionProfile.placeAmbience"/>
/// (the Classic), the original's single ambience plays everywhere instead. On Managers/Gameplay.prefab
/// </summary>
public class AmbienceDirector : MonoBehaviour
{
    [SerializeField, Tooltip("The bottom of the rift")] private AK.Wwise.Event cave = new AK.Wwise.Event();
    [SerializeField, Tooltip("An open place, over the void")] private AK.Wwise.Event cliff = new AK.Wwise.Event();
    [SerializeField, Tooltip("Drareg's garden: the antechamber and the boss room")] private AK.Wwise.Event dream = new AK.Wwise.Event();
    [SerializeField, Tooltip("Laid over the place's loop in the rooms with pools")] private AK.Wwise.Event water = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event waterStop = new AK.Wwise.Event();
    [SerializeField, Tooltip("Stops the places' loops and the water")] private AK.Wwise.Event stop = new AK.Wwise.Event();
    [SerializeField, Tooltip("The original's single ambience, which each place's loop stops")] private AK.Wwise.Event original = new AK.Wwise.Event();

    // The loop playing, to post it again only when the place changes
    private AK.Wwise.Event current;
    private bool waterPlaying;

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
        Edition.Changed += OnEditionChanged;
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        Edition.Changed -= OnEditionChanged;
        if (current != null) stop.Post(gameObject);
        current = null;
        waterPlaying = false;
    }

    private void Start()
    {
        if (!Edition.Profile.placeAmbience) original.Post(gameObject);
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (Edition.Profile.placeAmbience) Play(room);
    }

    private void OnEditionChanged(GameEdition edition)
    {
        if (Edition.Profile.placeAmbience)
        {
            Room room = GameScene.Map.CurrentRoom;
            if (room != null) Play(room);
        }
        else if (current != null)
        {
            stop.Post(gameObject);
            current = null;
            waterPlaying = false;
            original.Post(gameObject);
        }
    }

    private void Play(Room room)
    {
        AK.Wwise.Event place = room.type is RoomType.ANTECHAMBER or RoomType.BOSS ? dream : room.place == RoomPlace.CLIFF ? cliff : cave;
        if (place != current)
        {
            place.Post(gameObject);
            current = place;
        }
        bool hasWater = room.GetComponentInChildren<WaterSurface>() != null;
        if (hasWater == waterPlaying) return;
        (hasWater ? water : waterStop).Post(gameObject);
        waterPlaying = hasWater;
    }
}
