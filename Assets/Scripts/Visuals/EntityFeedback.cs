using UnityEngine;

/// <summary>
/// Shows the hits and the death of an entity: hit VFX and animator triggers
/// </summary>
[RequireComponent(typeof(EntityStats))]
public class EntityFeedback : MonoBehaviour
{
    private static readonly int TakingDamage = Animator.StringToHash("isTakingDamage");
    private static readonly int DamageValue = Animator.StringToHash("DamageValue");
    private static readonly int Dying = Animator.StringToHash("isDying");

    [SerializeField, Tooltip("Height of the hit VFX")] private float hitVFXHeight;
    [SerializeField] private Animator animator;

    private EntityStats stats;

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
    }

    private void OnEnable()
    {
        stats.Hit += OnHit;
        stats.Died += OnDied;
    }

    private void OnDisable()
    {
        stats.Hit -= OnHit;
        stats.Died -= OnDied;
    }

    private void OnHit(int healthLost)
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = hitVFXHeight;
        Destroy(Instantiate(GameAssets.Instance.hit, spawnPosition, Quaternion.identity), 0.5f);

        if (animator == null) return;
        animator.SetTrigger(TakingDamage);
        animator.SetInteger(DamageValue, healthLost);
    }

    private void OnDied()
    {
        if (animator != null) animator.SetTrigger(Dying);
    }
}
