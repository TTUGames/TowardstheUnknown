using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The weight of the hits: shakes the camera by the damage, freezes the time for an instant on hits and kills,
/// and slows it down on the last kill of a combat. Listens to the entities' damage and deaths
/// </summary>
public class ImpactFeedback : MonoBehaviour
{
    [SerializeField, Required, Tooltip("Shaken around its position and rotation of the start")] private Transform shakenCamera;

    [BoxGroup("Shake"), SerializeField, Tooltip("Offset at full trauma, in meters")] private float maxOffset = 0.2f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Roll at full trauma, in degrees")] private float maxRoll = 1.5f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Noise speed")] private float frequency = 22f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma lost per second, trauma going from 0 to 1")] private float recovery = 1.6f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma per health point the player loses")] private float traumaPerPlayerHealthLost = 1 / 45f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma when an enemy loses health")] private float enemyHitTrauma = 0.3f;
    [BoxGroup("Shake"), SerializeField, Tooltip("Trauma when the armor takes all of a hit")] private float blockedHitTrauma = 0.1f;
    [BoxGroup("Shake"), SerializeField] private float killTrauma = 0.35f;
    [BoxGroup("Shake"), SerializeField] private float bossPhaseTrauma = 0.8f;

    [BoxGroup("Hit stop"), SerializeField, SuffixLabel("s")] private float hitStop = 0.05f;
    [BoxGroup("Hit stop"), SerializeField, SuffixLabel("s")] private float playerHitStop = 0.08f;
    [BoxGroup("Hit stop"), SerializeField, SuffixLabel("s")] private float killHitStop = 0.12f;

    [BoxGroup("Last kill"), SerializeField, Range(0.05f, 1)] private float lastKillTimeScale = 0.35f;
    [BoxGroup("Last kill"), SerializeField, SuffixLabel("s")] private float lastKillDuration = 0.7f;

    private float trauma;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float seed;

    private void Awake()
    {
        startPosition = shakenCamera.localPosition;
        startRotation = shakenCamera.localRotation;
        seed = Random.value * 100;
    }

    private void OnEnable()
    {
        EntityStats.AnyDamageTaken += OnDamageTaken;
        GameEvents.EntityDied += OnEntityDied;
        GameEvents.BossPhaseChanged += OnBossPhaseChanged;
    }

    private void OnDisable()
    {
        EntityStats.AnyDamageTaken -= OnDamageTaken;
        GameEvents.EntityDied -= OnEntityDied;
        GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
    }

    /// <summary>
    /// Adds to the shake, which decreases by itself. Trauma goes from 0 to 1
    /// </summary>
    public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        bool isPlayer = entity.type == EntityType.PLAYER;
        if (healthLost <= 0)
        {
            AddTrauma(blockedHitTrauma);
            return;
        }
        AddTrauma(isPlayer ? healthLost * traumaPerPlayerHealthLost : enemyHitTrauma);
        GameTime.HitStop(isPlayer ? playerHitStop : hitStop);
    }

    private void OnEntityDied(EntityStats entity)
    {
        AddTrauma(killTrauma);
        GameTime.HitStop(killHitStop);
        if (entity.type != EntityType.PLAYER && IsLastEnemy(entity))
            GameTime.SlowMotion(lastKillTimeScale, lastKillDuration);
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
        if (shake <= 0)
        {
            shakenCamera.SetLocalPositionAndRotation(startPosition, startRotation);
            return;
        }
        float time = Time.unscaledTime * frequency;
        Vector3 offset = new Vector3(Noise(seed, time), Noise(seed + 10, time), 0) * (maxOffset * shake);
        float roll = Noise(seed + 20, time) * maxRoll * shake;
        //The offset is along the camera's axes, so that the view moves on the screen plane
        shakenCamera.SetLocalPositionAndRotation(startPosition + startRotation * offset, startRotation * Quaternion.Euler(0, 0, roll));
    }

    private static float Noise(float seed, float time) => Mathf.PerlinNoise(seed, time) * 2 - 1;
}
