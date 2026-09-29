using System.Collections;
using UnityEngine;

/// <summary>
/// A VFX an ability shoots from its caster to the targeted tile: its arrival is the ability's impact, later the farther the target
/// </summary>
[System.Serializable]
public class ProjectileInfo
{
    [SerializeField, Tooltip("Flies from the origin to the target, none if empty. Its particles simulate in local space to follow it (world space trails stay behind) and outlive the flight")]
    private GameObject prefab;
    [SerializeField, Tooltip("Where it starts: a marker of the player's body, or the caster's tile (an enemy has no markers)")]
    private VFXInfo.Target origin = VFXInfo.Target.GUN;
    [SerializeField, Tooltip("Added to the origin's position, in world space: the height of an enemy's shot from its tile")]
    private Vector3 offset;
    [SerializeField, Min(0.1f), Tooltip("Units per second, a tile being 1 unit")]
    private float speed = 30;

    public GameObject Prefab => prefab;

    /// <summary>
    /// Flies the projectile from the caster to the body of the entity on the targeted tile, or to the tile's center at its
    /// starting height, and ends at its arrival. The attack releases it with its other VFX
    /// </summary>
    public IEnumerator Fly(AttackAnimationAction action, TacticsMove source, Tile targetTile)
    {
        Vector3 start = VFXInfo.Origin(origin, source, targetTile).position + offset;
        Vector3 path = Arrival(start, targetTile) - start;
        float distance = path.magnitude;
        //Its local -Z faces the target, as the VFX of VFXInfo
        Quaternion rotation = distance > 0 ? Quaternion.LookRotation(-path) : source.transform.rotation;
        GameObject projectile = VFXPool.Get(prefab, start, rotation);
        action.AddVFX(projectile);

        float travelled = 0;
        while (travelled < distance)
        {
            yield return null;
            travelled = Mathf.Min(distance, travelled + speed * Time.deltaTime);
            projectile.transform.position = start + path * (travelled / distance);
        }
    }

    /// <summary>
    /// The point of the target's body nearest to the start, or the tile's center at the start's height when nobody stands on it
    /// </summary>
    private static Vector3 Arrival(Vector3 start, Tile targetTile)
    {
        Vector3 center = targetTile.transform.position;
        center.y = start.y;
        TacticsMove entity = targetTile.GetEntity();
        if (entity == null) return center;

        Bounds? body = null;
        foreach (Collider collider in entity.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled || collider.isTrigger) continue;
            if (body is Bounds bounds)
            {
                bounds.Encapsulate(collider.bounds);
                body = bounds;
            }
            else body = collider.bounds;
        }
        return body is Bounds found ? found.ClosestPoint(start) : center;
    }
}
