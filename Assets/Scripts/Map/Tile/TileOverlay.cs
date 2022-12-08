using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileOverlay : MonoBehaviour
{
    [SerializeField] Material attackMaterial;
    [SerializeField] Material movementMaterial;
    [SerializeField] Material changemapMaterial;
    [SerializeField] Material targetMaterial;

    private MeshRenderer meshRenderer;

	private void Awake() {
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		meshRenderer.enabled = false;
	}

	public void SetSelectable(Tile.SelectionType selectionType) {
		if (selectionType == Tile.SelectionType.NONE) {
			meshRenderer.enabled = false;
			return;
		}
		meshRenderer.enabled = true;
		switch(selectionType) {
			case Tile.SelectionType.ATTACK:
				meshRenderer.material = attackMaterial;
				break;
			case Tile.SelectionType.MOVEMENT:
				meshRenderer.material = movementMaterial;
				break;
		}
	}

	public void SetTarget() {
		meshRenderer.enabled = true;
		meshRenderer.material = targetMaterial;
	}
}
