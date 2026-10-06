using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The weight of the hits: shakes the camera and freezes the time for an instant, both scaled by the health a hit takes,
/// more on kills, and slows the time down and zooms in towards the last kill of a combat, and on the player's body at its death,
/// the colors fading, until the results (<see cref="EditionProfile.defeatBeat"/>). Listens to the entities' damage and deaths.
/// The one writer of the camera's transform and size: it shakes around the rest moved by <see cref="TurnCameraFocus"/>.
/// Each part follows its setting of the edition's profile (<see cref="EditionProfile.impactShake"/>, <c>hitStop</c>,
/// <c>finisherSlowMotion</c>, <c>finisherZoom</c>, <c>finisherSound</c>; the hit stops, the slow motion and the zoom also the
/// accessibility setting ReduceImpact). With the shake off (the Classic), only the original's shake plays, on the
/// hits taking the player's health (<see cref="EditionProfile.playerHitShake"/>)
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

    [BoxGroup("Kick"), SerializeField, SuffixLabel("m"), Tooltip("The camera jolts along a hit, from the attacker to the one hit: its distance for the lightest hit taking health")] private float lightKick = 0.03f;
    [BoxGroup("Kick"), SerializeField, SuffixLabel("m"), Tooltip("Of a heavy hit")] private float heavyKick = 0.1f;
    [BoxGroup("Kick"), SerializeField, Range(0, 0.1f), Tooltip("Share of the view the camera zooms in by with a heavy hit's jolt")] private float heavyKickZoom = 0.025f;
    [BoxGroup("Kick"), SerializeField, SuffixLabel("s"), Tooltip("In real seconds: out at once, back eased")] private float kickDuration = 0.22f;

    [BoxGroup("Last kill"), SerializeField, Range(0.05f, 1)] private float lastKillTimeScale = 0.25f;
    [BoxGroup("Last kill"), SerializeField, SuffixLabel("s")] private float lastKillDuration = 1f;
    [BoxGroup("Last kill"), SerializeField, Range(0, 0.5f), Tooltip("Share of the view the camera zooms in by, during the slow motion")] private float lastKillZoom = 0.18f;
    [BoxGroup("Last kill"), SerializeField, Range(0, 1), Tooltip("Share of the distance from the middle of the screen to the kill the camera moves")] private float lastKillFocus = 0.4f;
    [BoxGroup("Last kill"), SerializeField, SuffixLabel("s"), Tooltip("In real seconds")] private float lastKillZoomIn = 0.12f;
    [BoxGroup("Last kill"), SerializeField, SuffixLabel("s"), Tooltip("In real seconds, once the slow motion is over")] private float lastKillZoomOut = 0.8f;
    [BoxGroup("Last kill"), SerializeField, Tooltip("The finisher's accent, over the attack's sound, its tail stretching through the slow motion")] private AK.Wwise.Event lastKillSound = new AK.Wwise.Event();

    [BoxGroup("Defeat"), SerializeField, Range(0.05f, 1), Tooltip("Time scale through the defeat's beat, from the player's death to the results")] private float defeatTimeScale = 0.3f;
    [BoxGroup("Defeat"), SerializeField, Range(0, 0.5f), Tooltip("Share of the view the camera zooms in by on the player's body, held under the results")] private float defeatZoom = 0.25f;
    [BoxGroup("Defeat"), SerializeField, SuffixLabel("s"), Tooltip("In real seconds")] private float defeatZoomIn = 0.6f;
    [BoxGroup("Defeat"), SerializeField, Range(-100, 0), Tooltip("Saturation the colors fade to over the beat (the color adjustments' scale), kept under the results")] private float defeatSaturation = -80;

    [BoxGroup("Original shake"), SerializeField, Tooltip("The original release's shake of a hit on the player: sideways offset in meters over its seconds (its Screenshake animation)")]
    private AnimationCurve originalShake = new(new Keyframe(0, 0), new Keyframe(0.1166667f, 0), new Keyframe(0.1333333f, 0.3f),
        new Keyframe(0.1666667f, 0), new Keyframe(0.2166667f, -0.5f), new Keyframe(0.25f, 0));
    private float originalShakeStart = float.NegativeInfinity;

    /// <summary>A hit taking health, with its weight (0 to 1, see <see cref="HitWeight"/>)</summary>
    public static event System.Action<EntityStats, float> HitWeighed;

    // The scale of the hits every feedback shares (HitWeight), from the rig's fields: read in Awake, which runs in both editions
    private static float heavyHealth = 40, playerWeight = 1.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        HitWeighed = null;
        heavyHealth = 40;
        playerWeight = 1.5f;
    }

    private float trauma;
    // The last hit's jolt: its direction in the camera parent's space, its distance and zoom, and its start in real time
    private Vector3 kickDirection;
    private float kickDistance, kickZoom, kickStart = float.NegativeInfinity;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float seed;
    private Camera zoomedCamera;
    private float restSize;
    // The last kill's zoom, in unscaled time
    private float zoomStart = float.NegativeInfinity;
    private Vector3 zoomShift;
    // The zoom playing: the last kill's or the defeat's
    private float zoomSize, zoomIn, zoomHold, zoomOut;
    // The defeat's fade of the colors, a volume made at the player's death: its start in real time and its length
    private Volume defeatVolume;
    private float defeatStart = float.NegativeInfinity, defeatLength;

    private void Awake()
    {
        heavyHealth = heavyHitHealth;
        playerWeight = playerHitWeight;
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
        GameEvents.RoomEntered += OnRoomEntered;
    }

    private void OnDisable()
    {
        GameEvents.DamageTaken -= OnDamageTaken;
        GameEvents.EntityDied -= OnEntityDied;
        GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
        GameEvents.RoomEntered -= OnRoomEntered;
        if (arrivalWipe != null) arrivalWipe.Revealing -= OnRoomRevealing;
        arrivalWipe = null;
        arrivalHeld = false;
        arrivalStart = float.NegativeInfinity;
        // Turned off (the Classic edition): the camera goes back to rest
        trauma = 0;
        zoomStart = float.NegativeInfinity;
        defeatStart = float.NegativeInfinity;
        if (defeatVolume != null) defeatVolume.weight = 0;
        originalShakeStart = float.NegativeInfinity;
        kickStart = float.NegativeInfinity;
        if (shakenCamera == null) return;
        shakenCamera.SetLocalPositionAndRotation(startPosition, startRotation);
        if (zoomedCamera != null) zoomedCamera.orthographicSize = restSize;
    }

    /// <summary>
    /// Adds to the shake, which decreases by itself. Trauma goes from 0 to 1
    /// </summary>
    public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

    // The hits and kills raise the trauma to their level instead of adding to it: an area hitting several entities at once
    // shakes like its heaviest hit
    private void RaiseTrauma(float amount) => trauma = Mathf.Clamp01(Mathf.Max(trauma, amount));

    /// <summary>
    /// How heavy a hit is, from 0 (the least health, or none) to 1 (<c>heavyHitHealth</c> or more, weighted on the player): the one
    /// scale of the hit stop, the shake, the hit animations and the enemies' flare, in both editions
    /// </summary>
    public static float HitWeight(EntityStats entity, int healthLost)
    {
        if (healthLost <= 0) return 0;
        float weight = (healthLost - 1) / Mathf.Max(1, heavyHealth - 1);
        if (entity != null && entity.type == EntityType.PLAYER) weight *= playerWeight;
        return Mathf.Clamp01(weight);
    }

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        EditionProfile profile = Edition.Profile;
        if (!profile.impactShake && profile.playerHitShake && healthLost > 0 && entity.type == EntityType.PLAYER)
            originalShakeStart = Time.unscaledTime;
        if (!profile.impactShake && !profile.hitStop) return;
        //A blocked hit, or a cost the entity paid itself, only nudges the camera
        if (healthLost <= 0 || entity.SelfInflictedHit && profile.lightSelfDamage)
        {
            if (profile.impactShake) RaiseTrauma(blockedHitTrauma);
            return;
        }
        float weight = HitWeight(entity, healthLost);
        HitWeighed?.Invoke(entity, weight);
        if (profile.hitStop && !GameSettings.ReducedImpact) GameTime.HitStop(Mathf.Lerp(lightHitStop, heavyHitStop, weight));
        if (!profile.impactShake) return;
        RaiseTrauma(Mathf.Lerp(lightHitTrauma, heavyHitTrauma, weight));
        Kick(entity, weight);
    }

    /// <summary>
    /// Jolts the camera along the hit, from the entity playing its turn (the attacker) to the one hit, on the screen plane;
    /// none without an attacker
    /// </summary>
    private void Kick(EntityStats hit, float weight)
    {
        EntityTurn attacker = TurnSystem.Instance != null ? TurnSystem.Instance.Current : null;
        if (attacker == null || attacker.gameObject == hit.gameObject) return;
        Vector3 along = hit.transform.position - attacker.transform.position;
        Vector3 forward = shakenCamera.forward;
        Vector3 onScreen = along - Vector3.Dot(along, forward) * forward;
        if (onScreen.sqrMagnitude < 1e-4f) return;
        Vector3 local = shakenCamera.parent != null ? shakenCamera.parent.InverseTransformVector(onScreen) : onScreen;
        kickDirection = local.normalized;
        kickDistance = Mathf.Lerp(lightKick, heavyKick, weight) * GameSettings.ScreenShake;
        kickZoom = GameSettings.ReducedImpact ? 0 : heavyKickZoom * weight * GameSettings.ScreenShake;
        kickStart = Time.unscaledTime;
    }

    // 0 at rest, 1 at the jolt's peak: out in its first tenth, back eased over the rest
    private float KickAmount()
    {
        float time = (Time.unscaledTime - kickStart) / kickDuration;
        if (time < 0 || time >= 1) return 0;
        if (time < 0.1f) return time / 0.1f;
        float back = 1 - (time - 0.1f) / 0.9f;
        return back * back;
    }

    private void OnEntityDied(EntityStats entity)
    {
        EditionProfile profile = Edition.Profile;
        if (profile.impactShake) RaiseTrauma(killTrauma);
        //The accessibility setting keeps the shake, which has its own, and the finisher's sound, and drops the time and zoom effects
        bool reduced = GameSettings.ReducedImpact;
        if (profile.hitStop && !reduced) GameTime.HitStop(killHitStop);
        if (entity.type != EntityType.PLAYER && IsLastEnemy(entity))
        {
            if (profile.finisherSlowMotion && !reduced) GameTime.SlowMotion(lastKillTimeScale, lastKillDuration);
            if (profile.finisherZoom && !reduced) ZoomOn(entity.transform.position, lastKillZoom, lastKillZoomIn, lastKillDuration, lastKillZoomOut);
            if (profile.finisherSound) lastKillSound.Post(gameObject);
        }
        else if (entity.type == EntityType.PLAYER && profile.defeatBeat > 0)
            PlayDefeat(entity.transform.position, profile.defeatBeat);
    }

    /// <summary>
    /// The defeat's beat: slows the time, zooms in on the body for good and fades the colors, until the results
    /// </summary>
    private void PlayDefeat(Vector3 body, float beat)
    {
        GameTime.SlowMotion(defeatTimeScale, beat);
        ZoomOn(body, defeatZoom, defeatZoomIn, float.PositiveInfinity, 0);
        if (defeatVolume == null)
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.Add<ColorAdjustments>().saturation.Override(defeatSaturation);
            defeatVolume = gameObject.AddComponent<Volume>();
            defeatVolume.isGlobal = true;
            defeatVolume.priority = 100;
            defeatVolume.sharedProfile = profile;
        }
        defeatVolume.weight = 0;
        defeatStart = Time.unscaledTime;
        defeatLength = beat;
    }

    private void OnDestroy()
    {
        if (defeatVolume != null) Destroy(defeatVolume.sharedProfile);
    }

    /// <summary>
    /// Zooms in towards a point of the board by a share of the view, in, held and out over real seconds
    /// </summary>
    private void ZoomOn(Vector3 point, float size, float timeIn, float hold, float timeOut)
    {
        zoomSize = size;
        zoomIn = timeIn;
        zoomHold = hold;
        zoomOut = timeOut;
        if (zoomedCamera == null) return;
        Vector3 toPoint = point - shakenCamera.position;
        Vector3 forward = shakenCamera.forward;
        Vector3 onScreen = (toPoint - Vector3.Dot(toPoint, forward) * forward) * lastKillFocus;
        zoomShift = shakenCamera.parent != null ? shakenCamera.parent.InverseTransformVector(onScreen) : onScreen;
        zoomStart = Time.unscaledTime;
    }

    // 0 at rest, 1 zoomed in
    // A room's arrival: the camera held closer while the wipe covers the screen (RoomEntered), back to rest as it reveals the
    // room (the wipe's Revealing); not with the reduced impact effects
    private SlantedWipe arrivalWipe;
    private bool arrivalHeld;
    private float arrivalStart = float.NegativeInfinity;

    private void OnRoomEntered(Room room, bool firstVisit)
    {
        if (Edition.Profile.arrivalZoom <= 0 || GameSettings.ReducedImpact || GameScene.UI == null) return;
        SlantedWipe wipe = GameScene.UI.Fade;
        // Only behind the room wipe: the first room of a scene shows without it
        if (wipe == null || !wipe.IsCovered) return;
        if (arrivalWipe != wipe)
        {
            if (arrivalWipe != null) arrivalWipe.Revealing -= OnRoomRevealing;
            arrivalWipe = wipe;
            arrivalWipe.Revealing += OnRoomRevealing;
        }
        arrivalHeld = true;
    }

    private void OnRoomRevealing()
    {
        if (!arrivalHeld) return;
        arrivalHeld = false;
        arrivalStart = Time.unscaledTime;
    }

    private float ArrivalAmount()
    {
        if (arrivalHeld) return 1;
        float time = (Time.unscaledTime - arrivalStart) / Edition.Profile.arrivalDuration;
        return time < 1 ? Mathf.SmoothStep(1, 0, time) : 0;
    }

    private float ZoomAmount()
    {
        float time = Time.unscaledTime - zoomStart;
        if (time < zoomIn) return Mathf.SmoothStep(0, 1, time / zoomIn);
        time -= zoomIn + zoomHold;
        if (time < 0) return 1;
        return time < zoomOut ? Mathf.SmoothStep(1, 0, time / zoomOut) : 0;
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

    private void OnBossPhaseChanged(int phase)
    {
        if (Edition.Profile.impactShake) AddTrauma(bossPhaseTrauma);
    }

    // In unscaled time: the camera keeps shaking through the hit stops
    private void LateUpdate()
    {
        if (defeatVolume != null && defeatStart > float.NegativeInfinity)
            defeatVolume.weight = Mathf.SmoothStep(0, 1, (Time.unscaledTime - defeatStart) / Mathf.Max(defeatLength, 0.01f));
        trauma = Mathf.Max(0, trauma - recovery * Time.unscaledDeltaTime);
        float shake = trauma * trauma * GameSettings.ScreenShake;
        Vector3 rest = turnFocus != null ? startPosition + turnFocus.Offset : startPosition;
        //The board framed higher (EditionProfile.cameraLift): the view moves down its own up axis, by a share of its height
        if (zoomedCamera != null) rest += startRotation * Vector3.down * (Edition.Profile.cameraLift * 2 * restSize);
        float kick = KickAmount();
        rest += kickDirection * (kickDistance * kick);
        if (zoomedCamera != null)
        {
            float zoom = ZoomAmount();
            zoomedCamera.orthographicSize = restSize * (1 - zoomSize * zoom - kickZoom * kick - Edition.Profile.arrivalZoom * ArrivalAmount());
            rest += zoomShift * zoom;
        }
        // The original's shake: its clip moved the camera's parent along its X (diagonal on the screen), crossfaded in over
        // its whole length from an Idle writing the defaults
        float originalTime = Time.unscaledTime - originalShakeStart;
        float originalLength = originalShake[originalShake.length - 1].time;
        if (originalTime >= 0 && originalTime <= originalLength)
            rest += Vector3.right * (originalShake.Evaluate(originalTime) * Mathf.Clamp01(originalTime / originalLength) * GameSettings.ScreenShake);
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
