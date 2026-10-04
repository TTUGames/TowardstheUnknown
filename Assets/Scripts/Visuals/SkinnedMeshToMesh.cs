using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Feeds a VFX graph the vertices of a skinned mesh in its current pose (Drareg's second phase body): baked every
/// <see cref="refreshRate"/> seconds at rest, every <see cref="activeRefreshRate"/> while the entity moves or attacks
/// </summary>
public class SkinnedMeshToMesh : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;
    public VisualEffect VFXGraph;
    [Tooltip("Seconds between two bakes at rest")] public float refreshRate;
    [Tooltip("Seconds between two bakes while the entity moves or attacks, so that the particles follow its body")] public float activeRefreshRate = 1 / 30f;

    // Made once and refilled each refresh: the pose baked, then its vertices alone, which the graph samples
    private Mesh baked;
    private Mesh points;
    private readonly List<Vector3> vertices = new();
    private EntityAnimator entityAnimator;
    private TacticsMove move;
    private float nextBake;

    private void Start()
    {
        baked = new Mesh();
        points = new Mesh();
        entityAnimator = GetComponentInParent<EntityAnimator>();
        move = GetComponentInParent<TacticsMove>();
    }

    private void OnDestroy()
    {
        Destroy(baked);
        Destroy(points);
    }

    /// <summary>
    /// Whether the body is in motion: walking, or playing an attack
    /// </summary>
    private bool Active => (entityAnimator != null && entityAnimator.IsAttacking) || (move != null && move.isMoving);

    private void Update()
    {
        if (Time.time < nextBake) return;
        nextBake = Time.time + (Active ? activeRefreshRate : refreshRate);
        skinnedMesh.BakeMesh(baked);
        baked.GetVertices(vertices);
        points.SetVertices(vertices);
        VFXGraph.SetMesh("Mesh", points);
    }
}
