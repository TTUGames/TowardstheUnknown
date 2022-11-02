using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightConstraint : TileConstraint {
	public override bool isValid(Tile origin, Tile tile) {
		RaycastHit hit;
		Physics.Raycast(origin.transform.position + Vector3.up, tile.transform.position - origin.transform.position, out hit, (tile.transform.position - origin.transform.position).magnitude);
		return hit.collider == null || (hit.collider.GetComponent<EntityStats>() != null && hit.collider.GetComponent<EntityStats>() == tile.GetEntity());
	}
}
