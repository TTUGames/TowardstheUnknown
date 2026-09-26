using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// A stalactite dripping into the water under it: now and then a drop (the particle system) falls and, as it reaches the
/// surface, rings spread on the water (<see cref="WaterRipples"/>) and its sound plays. Placed in a room over a pool
/// </summary>
public class WaterDrip : MonoBehaviour
{
    [SerializeField, Tooltip("Emits one drop falling from here, with gravity")] private ParticleSystem drop;
    [SerializeField, MinMaxSlider(0.5f, 20, true), SuffixLabel("s"), Tooltip("Between two drops")] private Vector2 interval = new(2.5f, 6);
    [SerializeField, Tooltip("The pools the drop can fall into")] private LayerMask waterMask;
    [SerializeField, SuffixLabel("m")] private float maxFall = 20;
    [SerializeField] private AK.Wwise.Event dripSound;

    private Vector3 landing;
    private float fallTime;
    private float nextDrop;
    private float landAt = float.MaxValue;

    private void Start()
    {
        // Queries the triggers too: the pools' colliders may be triggers
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxFall, waterMask, QueryTriggerInteraction.Collide))
        {
            enabled = false;
            return;
        }
        landing = hit.point;
        // The time a drop takes to fall there, with the particle system's gravity
        float gravity = drop != null ? Mathf.Max(drop.main.gravityModifierMultiplier * -Physics.gravity.y, 0.01f) : -Physics.gravity.y;
        fallTime = Mathf.Sqrt(2 * hit.distance / gravity);
        nextDrop = Time.time + Random.Range(0, interval.y);
    }

    private void Update()
    {
        if (Time.time >= nextDrop)
        {
            if (drop != null) drop.Emit(1);
            landAt = Time.time + fallTime;
            nextDrop = Time.time + Random.Range(interval.x, interval.y);
        }
        if (Time.time >= landAt)
        {
            landAt = float.MaxValue;
            WaterRipples.Add(landing);
            if (dripSound != null && dripSound.IsValid()) dripSound.Post(gameObject);
        }
    }
}
