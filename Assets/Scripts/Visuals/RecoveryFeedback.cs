using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Motes rising from an entity's body when it heals (in the heal color) or gains armor (in the shield color),
/// from the game events (<see cref="EntityParticles"/>)
/// </summary>
public class RecoveryFeedback : MonoBehaviour
{
    [SerializeField, Required, AssetsOnly, Tooltip("From the body; its box shape takes the body's size")] private ParticleSystem motes;
    [SerializeField, ColorUsage(false), Tooltip("The UI's --color-heal. The particles' material holds the brightness")] private Color healColor = new(0.12f, 1f, 0.1f);
    [SerializeField, ColorUsage(false), Tooltip("The UI's --color-shield")] private Color armorColor = new(0.1f, 0.77f, 1f);

    private void OnEnable()
    {
        GameEvents.Healed += OnHealed;
        GameEvents.ArmorGained += OnArmorGained;
    }

    private void OnDisable()
    {
        GameEvents.Healed -= OnHealed;
        GameEvents.ArmorGained -= OnArmorGained;
    }

    private void OnHealed(EntityStats entity, int healed)
    {
        if (healed > 0) EntityParticles.Play(motes, entity.gameObject, healColor);
    }

    private void OnArmorGained(EntityStats entity, int armor)
    {
        if (armor > 0) EntityParticles.Play(motes, entity.gameObject, armorColor);
    }
}
