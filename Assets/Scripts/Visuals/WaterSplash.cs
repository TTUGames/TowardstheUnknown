using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On a snowfall's particle system: a flake falling into a pool (a <see cref="WaterSurface"/>'s collider, listed in the
/// system's Trigger module as the pools come and go) vanishes and leaves a small ring on the water (<see cref="WaterRipples"/>)
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class WaterSplash : MonoBehaviour
{
    private ParticleSystem system;
    private readonly List<ParticleSystem.Particle> entered = new();
    private int version = -1;

    private void Awake() => system = GetComponent<ParticleSystem>();

    private void OnEnable()
    {
        ParticleSystem.TriggerModule trigger = system.trigger;
        trigger.enabled = true;
        trigger.enter = ParticleSystemOverlapAction.Callback;
        trigger.inside = ParticleSystemOverlapAction.Ignore;
        trigger.exit = ParticleSystemOverlapAction.Ignore;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        version = -1;
    }

    private void Update()
    {
        if (version != WaterSurface.Version) ListPools();
        WaterRipples.Flush();
    }

    private void ListPools()
    {
        version = WaterSurface.Version;
        ParticleSystem.TriggerModule trigger = system.trigger;
        for (int i = trigger.colliderCount - 1; i >= 0; i--) trigger.RemoveCollider(i);
        foreach (WaterSurface pool in WaterSurface.Active)
            if (pool.Collider != null) trigger.AddCollider(pool.Collider);
    }

    private void OnParticleTrigger()
    {
        int count = system.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, entered);
        bool world = system.main.simulationSpace == ParticleSystemSimulationSpace.World;
        for (int i = 0; i < count; i++)
        {
            ParticleSystem.Particle flake = entered[i];
            WaterRipples.Splash(world ? flake.position : transform.TransformPoint(flake.position));
            flake.remainingLifetime = 0;
            entered[i] = flake;
        }
        system.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, entered);
    }
}
