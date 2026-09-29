using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Plays the VFX graphs and particle systems under it at another rate, so that an effect timed for an attack lands on the impact
/// of its retimed attack. A nested <c>VFXPlayRate</c> sets the rate of what it holds (1 keeps an added part at its own time)
/// </summary>
public class VFXPlayRate : MonoBehaviour
{
    [Min(0.05f), Tooltip("Speed of the effects: 1.3 plays an effect set for 0.47 s at 0.36 s")]
    public float playRate = 1;

    // The particle systems it holds and their own simulation speed, which the rate multiplies
    private List<(ParticleSystem system, float speed)> systems;

    private void OnEnable()
    {
        foreach (VisualEffect effect in GetComponentsInChildren<VisualEffect>(true))
            if (effect.GetComponentInParent<VFXPlayRate>(true) == this) effect.playRate = playRate;
        systems ??= GetComponentsInChildren<ParticleSystem>(true).Where(system => system.GetComponentInParent<VFXPlayRate>(true) == this)
            .Select(system => (system, system.main.simulationSpeed)).ToList();
        foreach ((ParticleSystem system, float speed) in systems)
        {
            ParticleSystem.MainModule main = system.main;
            main.simulationSpeed = speed * playRate;
        }
    }
}
