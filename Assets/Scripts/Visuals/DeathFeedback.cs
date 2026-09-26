using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The enemies' deaths in particles of their energy: a burst from the body when the lethal hit lands, then motes rising
/// from the corpse as it vanishes (<see cref="EntityFeedback.VanishStarted"/>). Tinted by the entity's energy color,
/// the <c>_GlowColor</c> of its materials, and sized by its body (<see cref="EntityParticles"/>)
/// </summary>
public class DeathFeedback : MonoBehaviour
{
    [SerializeField, Required, AssetsOnly, Tooltip("At the middle of the body when it dies; its shape's radius is scaled by the body's width")] private ParticleSystem burst;
    [SerializeField, Required, AssetsOnly, Tooltip("From the corpse as it vanishes; its box shape takes the body's size")] private ParticleSystem dissipation;
    [SerializeField, ColorUsage(false), Tooltip("For an entity without an energy color. The particles' material holds the brightness: their colors are clamped to 1")] private Color fallbackColor = new(0.9f, 0.9f, 1f);

    private void OnEnable()
    {
        GameEvents.EntityDied += OnEntityDied;
        EntityFeedback.VanishStarted += OnVanishStarted;
    }

    private void OnDisable()
    {
        GameEvents.EntityDied -= OnEntityDied;
        EntityFeedback.VanishStarted -= OnVanishStarted;
    }

    private void OnEntityDied(EntityStats entity)
    {
        if (entity.type == EntityType.PLAYER) return;
        EntityParticles.Play(burst, entity.gameObject, EntityParticles.EnergyColor(entity.gameObject, fallbackColor));
    }

    private void OnVanishStarted(EntityFeedback corpse, float duration)
    {
        if (!corpse.TryGetComponent(out EntityStats entity) || entity.type == EntityType.PLAYER) return;
        EntityParticles.Play(dissipation, corpse.gameObject, EntityParticles.EnergyColor(corpse.gameObject, fallbackColor));
    }
}
