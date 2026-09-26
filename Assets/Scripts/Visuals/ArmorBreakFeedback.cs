using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// An entity's armor broken by a hit shatters like glass around its body: shards thrown out that tumble, fall and fade,
/// and a flash, in the shield color (<see cref="EntityParticles"/>)
/// </summary>
public class ArmorBreakFeedback : MonoBehaviour
{
    [SerializeField, Required, AssetsOnly, Tooltip("From the body; its sphere shape's radius takes the body's width")] private ParticleSystem shatter;
    [SerializeField, ColorUsage(false), Tooltip("The UI's --color-shield. The particles' material holds the brightness")] private Color shieldColor = new(0.1f, 0.77f, 1f);

    private void OnEnable() => GameEvents.DamageTaken += OnDamageTaken;

    private void OnDisable() => GameEvents.DamageTaken -= OnDamageTaken;

    // Raised once the armor took its part: the armor absorbed some of the hit and none is left
    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        if (damage > healthLost && entity.Armor <= 0) EntityParticles.Play(shatter, entity.gameObject, shieldColor);
    }
}
