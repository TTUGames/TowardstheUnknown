using System;
using UnityEngine;

/// <summary>
/// The particle systems the reduced particles setting thins (<see cref="GameSettings.ReducedParticles"/>): their emission,
/// rate and bursts, times their share, so that what passes in front of the board hides less of it; a share of 0 stops a
/// system emitting. Its particles already out end their lives
/// </summary>
public class ParticleReduction : MonoBehaviour
{
    [Serializable]
    private struct Thinned
    {
        public ParticleSystem system;
        [Range(0, 1), Tooltip("Share of its emission kept with the setting on")] public float share;
    }

    [SerializeField] private Thinned[] systems = Array.Empty<Thinned>();

    // The emissions as the prefab sets them
    private float[] rates;
    private ParticleSystem.Burst[][] bursts;

    private void Awake()
    {
        rates = new float[systems.Length];
        bursts = new ParticleSystem.Burst[systems.Length][];
        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i].system == null) continue;
            ParticleSystem.EmissionModule emission = systems[i].system.emission;
            rates[i] = emission.rateOverTimeMultiplier;
            bursts[i] = new ParticleSystem.Burst[emission.burstCount];
            emission.GetBursts(bursts[i]);
        }
    }

    private void OnEnable()
    {
        Apply();
        GameSettings.Changed += OnSettingChanged;
    }

    private void OnDisable() => GameSettings.Changed -= OnSettingChanged;

    private void OnSettingChanged(GameSetting setting)
    {
        if (setting == GameSetting.ReducedParticles) Apply();
    }

    private void Apply()
    {
        for (int i = 0; i < systems.Length; i++)
        {
            if (systems[i].system == null) continue;
            float share = GameSettings.ReducedParticles ? systems[i].share : 1;
            ParticleSystem.EmissionModule emission = systems[i].system.emission;
            emission.rateOverTimeMultiplier = rates[i] * share;
            var scaled = (ParticleSystem.Burst[])bursts[i].Clone();
            for (int b = 0; b < scaled.Length; b++)
            {
                ParticleSystem.MinMaxCurve count = scaled[b].count;
                // constant and constantMax are the same value: scaled once, by its mode
                switch (count.mode)
                {
                    case ParticleSystemCurveMode.Constant:
                        count.constant *= share;
                        break;
                    case ParticleSystemCurveMode.TwoConstants:
                        count.constantMin *= share;
                        count.constantMax *= share;
                        break;
                    default:
                        count.curveMultiplier *= share;
                        break;
                }
                scaled[b].count = count;
            }
            emission.SetBursts(scaled);
        }
    }
}
