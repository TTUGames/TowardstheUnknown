using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Motes rising from an entity's body when it heals (in the UI's heal color) or gains armor (in its shield color),
/// from the game events (<see cref="EntityParticles"/>)
/// </summary>
public class RecoveryFeedback : MonoBehaviour
{
    [SerializeField, Required, AssetsOnly, Tooltip("From the body; its box shape takes the body's size")] private ParticleSystem motes;

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
        if (healed > 0) EntityParticles.Play(motes, entity.gameObject, EntityParticles.UIColor(Hud.HealColor));
    }

    private void OnArmorGained(EntityStats entity, int armor)
    {
        if (armor > 0) EntityParticles.Play(motes, entity.gameObject, EntityParticles.UIColor(Hud.ShieldColor));
    }
}
