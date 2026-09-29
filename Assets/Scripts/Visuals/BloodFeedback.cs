using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The blood of the hits: a spurt of streaks from the body of an entity losing health, thrown away from the attacker (the
/// entity playing its turn), more of them and faster on a heavier hit (<see cref="ImpactFeedback.HitWeighed"/>'s weight,
/// raised in the Anniversary only), then the marks they leave on the ground as they land: long streaks along the throw and a
/// splash at their root, fading out very slowly. The marks are placed by code (raycasts on the terrain), not by particle
/// collisions. An entity may throw something else and leave nothing (the Golem, shards of its crystal). The marks go when
/// the player leaves the room: the rooms share the same place in the world
/// </summary>
public class BloodFeedback : MonoBehaviour
{
    [System.Serializable]
    private class Variant
    {
        public EntityData entity;
        [Tooltip("Thrown instead of the blood, leaving no mark; none throws nothing")] public ParticleSystem spurt;
    }

    [SerializeField, Required, AssetsOnly, Tooltip("Played from the middle of the body, its forward thrown away from the attacker; its first burst's count and start speed are set per hit")]
    private ParticleSystem spurt;
    [SerializeField, Required, AssetsOnly, Tooltip("The marks on the ground: a world space system of streaks, instantiated once, which the hits emit into; its first child, the splashes")]
    private ParticleSystem marksPrefab;
    [SerializeField, Tooltip("Entities that throw something else than blood")] private Variant[] variants = System.Array.Empty<Variant>();
    [SerializeField, Tooltip("The colliders the marks lie on")] private LayerMask ground;

    [BoxGroup("Spurt"), SerializeField, Min(0), Tooltip("Streaks of the lightest hit taking health")] private int lightCount = 5;
    [BoxGroup("Spurt"), SerializeField, Min(0), Tooltip("Streaks of a heavy hit (hit weight 1)")] private int heavyCount = 14;
    [BoxGroup("Spurt"), SerializeField, SuffixLabel("m/s")] private float lightSpeed = 2f;
    [BoxGroup("Spurt"), SerializeField, SuffixLabel("m/s")] private float heavySpeed = 4.2f;
    [BoxGroup("Spurt"), SerializeField, Range(0, 1), Tooltip("How much the spurt rises: 0 straight away from the attacker, 1 straight up")] private float rise = 0.35f;

    [BoxGroup("Marks"), SerializeField, SuffixLabel("s"), Tooltip("From the hit to the marks, as the spurt lands")] private float landDelay = 0.3f;
    [BoxGroup("Marks"), SerializeField, Min(0), Tooltip("Streaks on the ground, light to heavy hit")] private Vector2Int streaks = new(2, 5);
    [BoxGroup("Marks"), SerializeField, SuffixLabel("m"), Tooltip("How far from the feet the streaks start, light to heavy hit")] private Vector2 reach = new(0.15f, 0.45f);
    [BoxGroup("Marks"), SerializeField, SuffixLabel("m"), Tooltip("A streak's length, light to heavy hit")] private Vector2 streakLength = new(0.25f, 0.6f);
    [BoxGroup("Marks"), SerializeField, SuffixLabel("m")] private Vector2 streakWidth = new(0.07f, 0.13f);
    [BoxGroup("Marks"), SerializeField, SuffixLabel("m"), Tooltip("The splash at the streaks' root")] private Vector2 splashSize = new(0.18f, 0.3f);
    [BoxGroup("Marks"), SerializeField, Range(0, 90), Tooltip("How far the streaks spread around the throw")] private float spread = 35;
    [BoxGroup("Marks"), SerializeField, SuffixLabel("m"), Tooltip("Above the ground, against the depth fighting")] private float lift = 0.01f;

    private ParticleSystem marks;
    private ParticleSystem splashes;
    // The spurts still flying, cleared with the marks when the player leaves the room
    private readonly List<GameObject> playing = new();

    private void Awake()
    {
        marks = Instantiate(marksPrefab, transform);
        marks.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        splashes = marks.transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        ImpactFeedback.HitWeighed += OnHitWeighed;
        GameEvents.RoomLeft += ClearAll;
        if (marks != null) marks.Play();
    }

    private void OnDisable()
    {
        ImpactFeedback.HitWeighed -= OnHitWeighed;
        GameEvents.RoomLeft -= ClearAll;
        StopAllCoroutines();
        ClearAll();
    }

    private void OnHitWeighed(EntityStats entity, float weight)
    {
        ParticleSystem prefab = spurt;
        bool bleeds = true;
        foreach (Variant variant in variants)
            if (variant.entity == entity.Data)
            {
                prefab = variant.spurt;
                bleeds = false;
            }
        if (prefab == null) return;

        Bounds body = EntityParticles.BodyBounds(entity.gameObject);
        EntityTurn attacker = TurnSystem.Instance != null ? TurnSystem.Instance.Current : null;
        Vector3 away = attacker != null && attacker.gameObject != entity.gameObject ? entity.transform.position - attacker.transform.position : entity.transform.forward;
        away.y = 0;
        away = away.sqrMagnitude > 1e-4f ? away.normalized : Vector3.forward;
        Throw(prefab, body.center, Vector3.Slerp(away, Vector3.up, rise), weight);
        if (bleeds) StartCoroutine(Land(entity.transform.position, away, weight));
    }

    private void Throw(ParticleSystem prefab, Vector3 from, Vector3 direction, float weight)
    {
        GameObject instance = VFXPool.Get(prefab.gameObject, from, Quaternion.LookRotation(direction));
        ParticleSystem root = instance.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = root.main;
        if (root.emission.burstCount > 0)
        {
            ParticleSystem.Burst burst = root.emission.GetBurst(0);
            burst.count = Mathf.RoundToInt(Mathf.Lerp(lightCount, heavyCount, weight));
            root.emission.SetBurst(0, burst);
        }
        main.startSpeedMultiplier = Mathf.Lerp(lightSpeed, heavySpeed, weight);
        root.Clear(true);
        root.Play(true);
        playing.RemoveAll(played => played == null || !played.activeInHierarchy);
        playing.Add(instance);
        VFXPool.Release(instance, main.duration + main.startLifetime.constantMax + 0.5f);
    }

    // As the spurt lands: streaks along the throw on the ground in front of the feet, and a splash at their root
    private IEnumerator Land(Vector3 feet, Vector3 away, float weight)
    {
        yield return new WaitForSeconds(landDelay);
        Color color = marksPrefab.main.startColor.Evaluate(Random.value);
        float start = Mathf.Lerp(reach.x, reach.y, weight) * 0.4f;
        float splash = Mathf.Lerp(splashSize.x, splashSize.y, weight);
        Emit(splashes, feet + away * start, away, splash, splash, color);
        int count = Mathf.RoundToInt(Mathf.Lerp(streaks.x, streaks.y, weight));
        for (int i = 0; i < count; i++)
        {
            Vector3 direction = Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.up) * away;
            float length = Mathf.Lerp(streakLength.x, streakLength.y, weight) * Random.Range(0.6f, 1.2f);
            float distance = Mathf.Lerp(reach.x, reach.y, weight) * Random.Range(0.5f, 1.1f);
            // The streak starts at its distance and runs away along its direction: its center is half its length further
            Emit(marks, feet + direction * (distance + length * 0.5f), direction, Random.Range(streakWidth.x, streakWidth.y), length,
                marksPrefab.main.startColor.Evaluate(Random.value));
        }
    }

    // A mark lying on the terrain under the point, its length along the direction; none off the board. The system is a
    // horizontal billboard: its rotation turns the mark around the up axis
    private void Emit(ParticleSystem system, Vector3 point, Vector3 direction, float width, float length, Color color)
    {
        if (!Physics.Raycast(point + Vector3.up, Vector3.down, out RaycastHit hit, 3, ground)) return;
        var emit = new ParticleSystem.EmitParams
        {
            position = hit.point + hit.normal * lift,
            startSize3D = new Vector3(width, length, 1),
            rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg,
            startColor = color,
            applyShapeToPosition = false,
        };
        system.Emit(emit, 1);
    }

    private void ClearAll()
    {
        foreach (GameObject instance in playing)
            if (instance != null)
                foreach (ParticleSystem system in instance.GetComponentsInChildren<ParticleSystem>())
                    system.Clear();
        playing.Clear();
        if (marks != null) marks.Clear(true);
    }
}
