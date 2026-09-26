using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The weight of the hits: shakes the camera and freezes the time for an instant, both scaled by the health a hit takes,
/// more on kills, and slows the time down and zooms in towards the last kill of a combat. Listens to the entities' damage and deaths.
/// The one writer of the camera's transform and size: it shakes around the rest moved by <see cref="TurnCameraFocus"/>
/// </summary>
public class ImpactFeedback : MonoBehaviour
{
    [SerializeField, Required, Tooltip("Shaken around its position and rotation of the start")] private Transform shakenCamera;
    [SerializeField, Tooltip("Its offset moves the rest the camera shakes around: this component is the one writer of the camera's transform")] private TurnCameraFocus turnFocus;

    [BoxGroup("Shake"), SerializeField, Tooltip("Offset at full trauma, in meters")] private float maxOffset = 0.2f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Roll at full trauma, in degrees")] private float maxRoll = 1.5f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Noise speed")] private float frequency = 22f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma lost per second, trauma going from 0 to 1")] private float recovery = 1.6f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma of a hit taking the least health, the shake being the trauma squared")] private float lightHitTrauma = 0.15f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma of a heavy hit")] private float heavyHitTrauma = 0.55f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma when the armor takes all of a hit")] private float blockedHitTrauma = 0.1f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma of a kill, when stronger than the lethal hit's")] private float killTrauma = 0.65f;
    [BoxGroup("Shake"), SerializeField] private float bossPhaseTrauma = 0.8f;

    [BoxGroup("Hit weight"), SerializeField, Min(1), Tooltip("Health lost by a heavy hit: the shake and hit stop grow with the health lost up to it")] private float heavyHitHealth = 40;
    [BoxGroup("Hit weight"), SerializeField, Min(0), Tooltip("Hits on the player weigh this much more: the player feels the hits taken")] private float playerHitWeight = 1.5f;

    [BoxGroup("Hit stop"), SerializeField, SuffixLabel("s"), Tooltip("Of a hit taking the least health, in real seconds: about two frames")] private float lightHitStop = 0.035f;
    [BoxGroup("Hit stop"), SerializeField, SuffixLabel("s"), Tooltip("Of a heavy hit")] private float heavyHitStop = 0.09f;
    [BoxGroup("Hit stop"), SerializeField, SuffixLabel("s"), Tooltip("Of a lethal hit, when longer than the hit's own")] private float killHitStop = 0.12f;

    [BoxGroup("Last kill"), SerializeField, Range(0.05f, 1)] private float lastKillTimeScale = 0.35f;
    [BoxGroup("Last kill"), SerializeField, SuffixLabel("s")] private float lastKillDuration = 0.7f;
    [BoxGroup("Last kill"), SerializeField, Range(0, 0.5f), Tooltip("Share of the view the camera zooms in by, during the slow motion")] private float lastKillZoom = 0.12f;
    [BoxGroup("Last kill"), SerializeField, Range(0, 1), Tooltip("Share of the distance from the middle of the screen to the kill the camera moves")] private float lastKillFocus = 0.3f;
    [BoxGroup("Last kill"), SerializeField, SuffixLabel("s"), Tooltip("In real seconds")] private float lastKillZoomIn = 0.12f;
    [BoxGroup("Last kill"), SerializeField, SuffixLabel("s"), Tooltip("In real seconds, once the slow motion is over")] private float lastKillZoomOut = 0.6f;

    /// <summary>A hit taking health, with its weight (0 to 1, see <see cref="HitWeight"/>)</summary>
    public static event System.Action<EntityStats, float> HitWeighed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => HitWeighed = null;

    private float trauma;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float seed;
    private Camera zoomedCamera;
    private float restSize;
    // The last kill's zoom, in unscaled time
    private float zoomStart = float.NegativeInfinity;
    private Vector3 zoomShift;

    private void Awake()
    {
        startPosition = shakenCamera.localPosition;
        startRotation = shakenCamera.localRotation;
        seed = Random.value * 100;
        zoomedCamera = shakenCamera.GetComponent<Camera>();
        if (zoomedCamera != null) restSize = zoomedCamera.orthographicSize;
    }

    private void OnEnable()
    {
        GameEvents.DamageTaken += OnDamageTaken;
        GameEvents.EntityDied += OnEntityDied;
        GameEvents.BossPhaseChanged += OnBossPhaseChanged;
    }

    private void OnDisable()
    {
        GameEvents.DamageTaken -= OnDamageTaken;
        GameEvents.EntityDied -= OnEntityDied;
        GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
    }

    /// <summary>
    /// Adds to the shake, which decreases by itself. Trauma goes from 0 to 1
    /// </summary>
    public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

    // The hits and kills raise the trauma to their level instead of adding to it: an area hitting several entities at once
    // shakes like its heaviest hit
    private void RaiseTrauma(float amount) => trauma = Mathf.Clamp01(Mathf.Max(trauma, amount));

    /// <summary>
    /// How heavy a hit is, from 0 (the least health) to 1 (<c>heavyHitHealth</c> or more, weighted on the player)
    /// </summary>
    private float HitWeight(EntityStats entity, int healthLost)
    {
        float weight = (healthLost - 1) / Mathf.Max(1, heavyHitHealth - 1);
        if (entity.type == EntityType.PLAYER) weight *= playerHitWeight;
        return Mathf.Clamp01(weight);
    }

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        if (healthLost <= 0)
        {
            RaiseTrauma(blockedHitTrauma);
            return;
        }
        float weight = HitWeight(entity, healthLost);
        HitWeighed?.Invoke(entity, weight);
        RaiseTrauma(Mathf.Lerp(lightHitTrauma, heavyHitTrauma, weight));
        GameTime.HitStop(Mathf.Lerp(lightHitStop, heavyHitStop, weight));
    }

    private void OnEntityDied(EntityStats entity)
    {
        RaiseTrauma(killTrauma);
        GameTime.HitStop(killHitStop);
        if (entity.type != EntityType.PLAYER && IsLastEnemy(entity))
        {
            GameTime.SlowMotion(lastKillTimeScale, lastKillDuration);
            ZoomOn(entity.transform.position);
        }
    }

    /// <summary>
    /// Zooms in a little towards a point of the board, held through the last kill's slow motion
    /// </summary>
    private void ZoomOn(Vector3 point)
    {
        if (zoomedCamera == null) return;
        Vector3 toPoint = point - shakenCamera.position;
        Vector3 forward = shakenCamera.forward;
        Vector3 onScreen = (toPoint - Vector3.Dot(toPoint, forward) * forward) * lastKillFocus;
        zoomShift = shakenCamera.parent != null ? shakenCamera.parent.InverseTransformVector(onScreen) : onScreen;
        zoomStart = Time.unscaledTime;
    }

    // 0 at rest, 1 zoomed in
    private float ZoomAmount()
    {
        float time = Time.unscaledTime - zoomStart;
        if (time < lastKillZoomIn) return Mathf.SmoothStep(0, 1, time / lastKillZoomIn);
        time -= lastKillZoomIn + lastKillDuration;
        if (time < 0) return 1;
        return time < lastKillZoomOut ? Mathf.SmoothStep(1, 0, time / lastKillZoomOut) : 0;
    }

    // The dying entity is still in the turn order when its death is raised
    private static bool IsLastEnemy(EntityStats dying)
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        if (turnSystem == null || !turnSystem.IsCombat) return false;
        foreach (EntityTurn turn in turnSystem.Turns)
            if (turn.TryGetComponent(out EntityStats stats) && stats != dying && stats.type != EntityType.PLAYER && stats.CurrentHealth > 0)
                return false;
        return true;
    }

    private void OnBossPhaseChanged(int phase) => AddTrauma(bossPhaseTrauma);

    // In unscaled time: the camera keeps shaking through the hit stops
    private void LateUpdate()
    {
        trauma = Mathf.Max(0, trauma - recovery * Time.unscaledDeltaTime);
        float shake = trauma * trauma * GameSettings.ScreenShake;
        Vector3 rest = turnFocus != null ? startPosition + turnFocus.Offset : startPosition;
        if (zoomedCamera != null)
        {
            float zoom = ZoomAmount();
            zoomedCamera.orthographicSize = restSize * (1 - lastKillZoom * zoom);
            rest += zoomShift * zoom;
        }
        if (shake <= 0)
        {
            shakenCamera.SetLocalPositionAndRotation(rest, startRotation);
            return;
        }
        float time = Time.unscaledTime * frequency;
        Vector3 offset = new Vector3(Noise(seed, time), Noise(seed + 10, time), 0) * (maxOffset * shake);
        float roll = Noise(seed + 20, time) * maxRoll * shake;
        //The offset is along the camera's axes, so that the view moves on the screen plane
        shakenCamera.SetLocalPositionAndRotation(rest + startRotation * offset, startRotation * Quaternion.Euler(0, 0, roll));
    }

    private static float Noise(float seed, float time) => Mathf.PerlinNoise(seed, time) * 2 - 1;
}
