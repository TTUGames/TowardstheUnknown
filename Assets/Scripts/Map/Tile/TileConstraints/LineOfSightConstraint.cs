using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightConstraint : TileConstraint {
	public override bool isValid(Tile origin, Tile tile) {
		RaycastHit hit;
		Vector3 raycastOrigin = new Vector3(origin.transform.position.x, origin.GetComponent<Collider>().bounds.max.y + 0.1f, origin.transform.position.z);
		Physics.Raycast(raycastOrigin, tile.transform.position - origin.transform.position, out hit, (tile.transform.position - origin.transform.position).magnitude);
		if (hit.collider == null) return true;
		TacticsMove hitEntity = hit.collider.GetComponent<TacticsMove>();
		return hit.collider == null || (hit.collider.GetComponent<TacticsMove>() != null && hit.collider.GetComponent<TacticsMove>() == tile.GetEntity());
	}
}
