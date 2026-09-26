using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A pool of water: a cube scaled to the pool, drawn by Rendering/Water.shader. Scenery only, nothing enters it. Lists
/// itself while enabled, so that <see cref="WaterReflection"/> mirrors the room under the largest pool on screen and
/// the snowflakes falling into it splash (<see cref="WaterSplash"/>)
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class WaterSurface : MonoBehaviour
{
    private static readonly List<WaterSurface> active = new();

    /// <summary>The pools enabled now</summary>
    public static IReadOnlyList<WaterSurface> Active => active;

    /// <summary>Changes each time a pool is enabled or disabled</summary>
    public static int Version { get; private set; }

    private MeshRenderer meshRenderer;

    /// <summary>The box's collider, a trigger: the snowflakes vanish in it (<see cref="WaterSplash"/>)</summary>
    public Collider Collider { get; private set; }

    public bool IsVisible => meshRenderer != null && meshRenderer.enabled && meshRenderer.isVisible;

    /// <summary>The height of the surface at rest, the top of the box</summary>
    public float Height => transform.TransformPoint(0, 0.5f, 0).y;

    /// <summary>The area of the surface, in square meters</summary>
    public float Area
    {
        get
        {
            Vector3 scale = transform.lossyScale;
            return Mathf.Abs(scale.x * scale.z);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        active.Clear();
        Version = 0;
    }

    private void OnEnable()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Collider = GetComponent<Collider>();
        active.Add(this);
        Version++;
    }

    private void OnDisable()
    {
        active.Remove(this);
        Version++;
    }
}
