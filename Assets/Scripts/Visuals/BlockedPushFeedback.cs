using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// A push stopped short by a wall or an entity (<see cref="GameEvents.PushBlocked"/>) jolts: a puff of dust at the entity's feet
/// and a small shake (<see cref="ImpactFeedback.AddTrauma"/>), so that the player sees the push was cut. With
/// <see cref="EditionProfile.blockedPushShock"/>
/// </summary>
public class BlockedPushFeedback : MonoBehaviour
{
    [SerializeField, Required, AssetsOnly, Tooltip("The footsteps' dust: world space, no emission of its own")] private ParticleSystem dustPrefab;
    [SerializeField, Min(1)] private int count = 16;
    [SerializeField, Min(0.1f), Tooltip("Of the puffs")] private float scale = 1.8f;
    [SerializeField, Range(0, 1), Tooltip("Added to the camera's shake")] private float trauma = 0.25f;
    [SerializeField, Required] private ImpactFeedback impact;

    private ParticleSystem dust;

    private void Awake()
    {
        dust = Instantiate(dustPrefab, transform);
        dust.transform.localScale = Vector3.one * scale;
    }

    private void OnEnable() => GameEvents.PushBlocked += OnPushBlocked;

    private void OnDisable() => GameEvents.PushBlocked -= OnPushBlocked;

    private void OnPushBlocked(EntityStats entity)
    {
        if (!Edition.Profile.blockedPushShock || entity == null) return;
        dust.Emit(new ParticleSystem.EmitParams { position = entity.transform.position + Vector3.up * 0.05f, applyShapeToPosition = true }, count);
        impact.AddTrauma(trauma);
    }
}
