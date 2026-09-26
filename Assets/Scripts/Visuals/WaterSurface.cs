using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A pool of water: a cube scaled to the pool, drawn by Rendering/Water.shader. Scenery only, nothing enters it. Lists
/// itself while enabled, so that <see cref="WaterReflection"/> mirrors the room under the largest pool on screen and
/// the snowflakes falling into it splash (<see cref="WaterSplash"/>), and passes the pools' boxes to the shaders
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class WaterSurface : MonoBehaviour
{
    private static readonly List<WaterSurface> active = new();
    private static readonly int PoolsId = Shader.PropertyToID("_WaterPools");
    private static readonly int PoolAxesId = Shader.PropertyToID("_WaterPoolAxes");
    private static readonly int PoolCountId = Shader.PropertyToID("_WaterPoolCount");
    // Keep in step with WATER_MAX_POOLS in Water.hlsl
    private const int MaxPools = 8;
    private static readonly Vector4[] pools = new Vector4[MaxPools];
    private static readonly Vector4[] poolAxes = new Vector4[MaxPools];
    private static int passedVersion = -1;

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
        passedVersion = -1;
        Shader.SetGlobalInt(PoolCountId, 0);
    }

    private void OnEnable()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Collider = GetComponent<Collider>();
        active.Add(this);
        Version++;
    }

    // Placed by its room after being enabled: passed from its first frame
    private void Start() => Version++;

    private void OnDisable()
    {
        active.Remove(this);
        Version++;
    }

    private void LateUpdate()
    {
        if (passedVersion != Version) PassPools();
    }

    /// <summary>
    /// Passes the enabled pools' boxes to the shaders (Water.hlsl's _WaterPools): Snow Lit wets the rock just over
    /// their waterline
    /// </summary>
    private static void PassPools()
    {
        passedVersion = Version;
        int count = 0;
        foreach (WaterSurface pool in active)
        {
            if (count == MaxPools) break;
            Transform t = pool.transform;
            Vector3 center = t.position, scale = t.lossyScale, right = t.right;
            Vector2 axis = new Vector2(right.x, right.z).normalized;
            pools[count] = new Vector4(center.x, center.z, Mathf.Abs(scale.x) * 0.5f, Mathf.Abs(scale.z) * 0.5f);
            poolAxes[count++] = new Vector4(axis.x, axis.y, pool.Height, 0);
        }
        Shader.SetGlobalVectorArray(PoolsId, pools);
        Shader.SetGlobalVectorArray(PoolAxesId, poolAxes);
        Shader.SetGlobalInt(PoolCountId, count);
    }
}
