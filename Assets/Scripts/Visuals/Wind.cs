using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The wind of the vegetation (Nature Lit, and Snow Lit with its wind on; see Rendering/Wind.hlsl): a breeze blowing the way the
/// snowfall drifts and, now and then, a gust whose front crosses the room, bending the plants as it passes, carrying its
/// effect (streaks and lifted snow) and pushing the snowflakes through its WindZone. Also passes to the shaders the entities of the room, which bend the grass and
/// the small plants around their feet, the room's drafts (<see cref="WindDraft"/>) and the waves sent by the heavy hits
/// </summary>
public class Wind : MonoBehaviour
{
    private static readonly int DirectionId = Shader.PropertyToID("_WindDirection");
    private static readonly int GustId = Shader.PropertyToID("_WindGust");
    private static readonly int FlutterId = Shader.PropertyToID("_WindFlutterParams");
    private static readonly int PushersId = Shader.PropertyToID("_WindPushers");
    private static readonly int PusherCountId = Shader.PropertyToID("_WindPusherCount");
    private static readonly int DraftsId = Shader.PropertyToID("_WindDrafts");
    private static readonly int DraftForcesId = Shader.PropertyToID("_WindDraftForces");
    private static readonly int DraftCountId = Shader.PropertyToID("_WindDraftCount");
    private static readonly int WavesId = Shader.PropertyToID("_WindWaves");
    private static readonly int WaveParamsId = Shader.PropertyToID("_WindWaveParams");
    private static readonly int WaveCountId = Shader.PropertyToID("_WindWaveCount");
    // Keep in step with the arrays of Wind.hlsl
    private const int MaxPushers = 16;
    private const int MaxDrafts = 8;
    private const int MaxWaves = 4;

    [BoxGroup("Breeze"), SerializeField, Tooltip("The way the wind blows, in degrees around the up axis from +X (the snowfall drifts toward +X)")] private float heading = -10;
    [BoxGroup("Breeze"), SerializeField, Range(0, 2), Tooltip("Scales every plant's bend and flutter: 0 is still")] private float strength = 1;
    [BoxGroup("Breeze"), SerializeField, Tooltip("The speed of the slow sway, in radians per second")] private float swaySpeed = 1.3f;
    [BoxGroup("Breeze"), SerializeField, Tooltip("The speed of the tips' flutter, in radians per second")] private float flutterSpeed = 9;

    [BoxGroup("Gusts"), SerializeField, Min(0), Tooltip("How much a gust adds to the lean: the breeze alone leans the plants by 0.3 of their bend")] private float gustStrength = 1.8f;
    [BoxGroup("Gusts"), SerializeField, MinMaxSlider(1, 40, true), SuffixLabel("s"), Tooltip("Between the end of a gust and the next")] private Vector2 gustInterval = new Vector2(11, 27);
    [BoxGroup("Gusts"), SerializeField, MinMaxSlider(0.2f, 2, true), Tooltip("Each gust is this much stronger or weaker, at random")] private Vector2 gustVariation = new Vector2(0.6f, 1.25f);
    [BoxGroup("Gusts"), SerializeField, Min(0.1f), SuffixLabel("m/s"), Tooltip("The speed of the front across the room")] private float gustSpeed = 6;
    [BoxGroup("Gusts"), SerializeField, Min(0.5f), SuffixLabel("m"), Tooltip("The width of the front")] private float gustWidth = 6;
    [BoxGroup("Gusts"), SerializeField, Tooltip("Carried by the front: its particles show the gust; a WindZone on it pushes the snowflakes")] private ParticleSystem gustEffectPrefab;
    [BoxGroup("Gusts"), SerializeField, Min(0), Tooltip("The WindZone's main strength at the heart of a gust")] private float gustWindZone = 1.2f;

    [BoxGroup("Entities"), SerializeField, Min(0), SuffixLabel("m"), Tooltip("The radius around an entity's feet in which the plants bend away")] private float entityRadius = 0.7f;

    [BoxGroup("Hit waves"), SerializeField, Min(1), Tooltip("A hit taking this much health or more sends a wave")] private int waveMinHealth = 8;
    [BoxGroup("Hit waves"), SerializeField, Min(1), Tooltip("A hit taking this much health sends the strongest wave")] private int waveHeavyHealth = 40;
    [BoxGroup("Hit waves"), SerializeField, Min(0), Tooltip("Of the lightest wave, times the plant's bend (or push)")] private float lightWave = 1.5f;
    [BoxGroup("Hit waves"), SerializeField, Min(0), Tooltip("Of the heaviest wave, and of a kill's")] private float heavyWave = 4;
    [BoxGroup("Hit waves"), SerializeField, Min(0.1f), SuffixLabel("m/s")] private float waveSpeed = 7;
    [BoxGroup("Hit waves"), SerializeField, Min(0.1f), SuffixLabel("s")] private float waveDuration = 1.2f;
    [BoxGroup("Hit waves"), SerializeField, Min(0.1f), SuffixLabel("m"), Tooltip("The width of the ring")] private float waveWidth = 1.2f;

    private readonly Vector4[] pushers = new Vector4[MaxPushers];
    private readonly Vector4[] drafts = new Vector4[MaxDrafts];
    private readonly Vector4[] draftForces = new Vector4[MaxDrafts];
    private readonly Vector4[] waves = new Vector4[MaxWaves];
    private readonly Vector4[] waveParams = new Vector4[MaxWaves];
    private WindDraft[] roomDrafts = new WindDraft[0];
    private int nextWave;
    // The room's extent along the wind, where the gust fronts start and end
    private Vector3 roomCenter;
    private float roomHalfLength = 10;
    private float gustFront = float.MaxValue;
    private float nextGust;
    private bool gustBlowing;
    private float gustScale = 1;
    private ParticleSystem gustEffect;
    private WindZone gustZone;

    private Vector3 Direction
    {
        get
        {
            float angle = heading * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        }
    }

    private void Start()
    {
        nextGust = Time.time + Random.Range(gustInterval.x, gustInterval.y) * 0.5f;
        if (gustEffectPrefab == null) return;
        gustEffect = Instantiate(gustEffectPrefab, transform);
        gustZone = gustEffect.GetComponent<WindZone>();
        if (gustZone != null) gustZone.windMain = 0;
    }

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
        EntityStats.AnyDamageTaken += OnDamageTaken;
        GameEvents.EntityDied += OnEntityDied;
        for (int i = 0; i < MaxWaves; i++) waves[i] = new Vector4(0, 0, 0, -1000);
        Shader.SetGlobalVectorArray(WavesId, waves);
        Shader.SetGlobalVectorArray(WaveParamsId, waveParams);
        Shader.SetGlobalInt(WaveCountId, MaxWaves);
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        EntityStats.AnyDamageTaken -= OnDamageTaken;
        GameEvents.EntityDied -= OnEntityDied;
        Shader.SetGlobalVector(DirectionId, Vector4.zero);
        Shader.SetGlobalInt(PusherCountId, 0);
        Shader.SetGlobalInt(DraftCountId, 0);
        Shader.SetGlobalInt(WaveCountId, 0);
    }

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        roomDrafts = room.GetComponentsInChildren<WindDraft>();
        Tile[] tiles = room.GetComponentsInChildren<Tile>();
        if (tiles.Length == 0) return;
        Bounds bounds = new Bounds(tiles[0].transform.position, Vector3.zero);
        foreach (Tile tile in tiles) bounds.Encapsulate(tile.transform.position);
        roomCenter = bounds.center;
        // Wide of the board: the front starts and ends out of the view
        roomHalfLength = Mathf.Abs(bounds.extents.x * Direction.x) + Mathf.Abs(bounds.extents.z * Direction.z) + 12;
    }

    private void Update()
    {
        Vector3 direction = Direction;
        Shader.SetGlobalVector(DirectionId, new Vector4(direction.x, 0, direction.z, strength));
        UpdateGust(direction);
        Shader.SetGlobalVector(FlutterId, new Vector4(flutterSpeed, 0, 0, 0));
        PassEntities();
        PassDrafts();
    }

    // In scaled time, as the shaders' clock: a gust stops with the hit stops
    private void UpdateGust(Vector3 direction)
    {
        float start = Vector3.Dot(roomCenter, direction) - roomHalfLength;
        if (!gustBlowing && Time.time >= nextGust)
        {
            gustBlowing = true;
            gustFront = start;
            gustScale = Random.Range(gustVariation.x, gustVariation.y);
            if (gustEffect != null) gustEffect.Play(true);
        }
        if (gustBlowing)
        {
            gustFront += gustSpeed * Time.deltaTime;
            if (gustFront > start + 2 * roomHalfLength)
            {
                gustBlowing = false;
                gustFront = float.MaxValue;
                nextGust = Time.time + Random.Range(gustInterval.x, gustInterval.y);
                if (gustEffect != null) gustEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        Shader.SetGlobalVector(GustId, new Vector4(gustStrength * gustScale, gustBlowing ? gustFront : -100000, gustWidth, swaySpeed));
        if (gustEffect == null) return;
        if (gustBlowing)
        {
            // The effect rides the front, across the room, facing the wind
            Vector3 center = roomCenter + direction * (gustFront - Vector3.Dot(roomCenter, direction));
            gustEffect.transform.SetPositionAndRotation(new Vector3(center.x, roomCenter.y, center.z), Quaternion.LookRotation(direction, Vector3.up));
        }
        if (gustZone != null)
        {
            float ramp = gustBlowing ? Mathf.Clamp01(1 - Mathf.Abs(gustFront - Vector3.Dot(roomCenter, direction)) / roomHalfLength) : 0;
            gustZone.windMain = gustWindZone * gustScale * ramp;
        }
    }

    private void PassEntities()
    {
        int count = 0;
        TurnSystem turnSystem = TurnSystem.Instance;
        if (turnSystem != null)
        {
            foreach (EntityTurn turn in turnSystem.Turns)
            {
                if (count == MaxPushers) break;
                if (turn == null || !turn.isActiveAndEnabled) continue;
                Vector3 feet = turn.transform.position;
                pushers[count++] = new Vector4(feet.x, feet.y, feet.z, entityRadius);
            }
        }
        Shader.SetGlobalVectorArray(PushersId, pushers);
        Shader.SetGlobalInt(PusherCountId, count);
    }

    private void PassDrafts()
    {
        int count = 0;
        foreach (WindDraft draft in roomDrafts)
        {
            if (count == MaxDrafts) break;
            if (draft == null || !draft.isActiveAndEnabled) continue;
            drafts[count] = draft.Area;
            draftForces[count++] = draft.Force;
        }
        Shader.SetGlobalVectorArray(DraftsId, drafts);
        Shader.SetGlobalVectorArray(DraftForcesId, draftForces);
        Shader.SetGlobalInt(DraftCountId, count);
    }

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        if (healthLost < waveMinHealth) return;
        float weight = Mathf.InverseLerp(waveMinHealth, waveHeavyHealth, healthLost);
        SendWave(entity.transform.position, Mathf.Lerp(lightWave, heavyWave, weight));
    }

    private void OnEntityDied(EntityStats entity) => SendWave(entity.transform.position, heavyWave);

    /// <summary>
    /// Sends a ring from a point that lays the plants down as it passes, the strength in multiples of their bend.
    /// The shaders' clock is the time since the scene loaded, scaled: the ring stops during a hit stop, as the wind does
    /// </summary>
    public void SendWave(Vector3 center, float waveStrength)
    {
        waves[nextWave] = new Vector4(center.x, center.y, center.z, Time.timeSinceLevelLoad);
        waveParams[nextWave] = new Vector4(waveStrength, waveSpeed, waveDuration, waveWidth);
        nextWave = (nextWave + 1) % MaxWaves;
        Shader.SetGlobalVectorArray(WavesId, waves);
        Shader.SetGlobalVectorArray(WaveParamsId, waveParams);
    }
}
