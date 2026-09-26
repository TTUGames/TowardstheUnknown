using UnityEngine;

/// <summary>
/// A condition on a tile of a <c>TileSearch</c>, seen from the search's origin
/// </summary>
public delegate bool TileConstraint(Tile origin, Tile tile);

public static class TileConstraints
{
    public static readonly TileConstraint Walkable = (origin, tile) => tile.isWalkable;

    /// <summary>
    /// No entity on the tile, but the origin's
    /// </summary>
    public static readonly TileConstraint Empty = (origin, tile) => origin == tile || tile.GetEntity() == null;

    /// <summary>
    /// Rejects the tiles holding a collectable, so that paths go around it: it is only reached when it is the destination
    /// </summary>
    public static readonly TileConstraint NoCollectable = (origin, tile) => origin == tile || tile.Collectable == null;

    /// <summary>
    /// Nothing between the origin and the tile, but the entity standing on it
    /// </summary>
    public static readonly TileConstraint LineOfSight = (origin, tile) => {
        Vector3 toTile = tile.transform.position - origin.transform.position;
        Vector3 raycastOrigin = new Vector3(origin.transform.position.x, origin.GetComponent<Collider>().bounds.max.y + 0.1f, origin.transform.position.z);
        if (!Physics.Raycast(raycastOrigin, toTile, out RaycastHit hit, toTile.magnitude)) return true;
        TacticsMove entity = hit.collider.GetComponent<TacticsMove>();
        return entity != null && entity == tile.GetEntity();
    };
}
