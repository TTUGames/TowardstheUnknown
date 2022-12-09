using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileOverlay : MonoBehaviour
{
    [SerializeField] Material attackMaterial;
    [SerializeField] Material movementMaterial;
    [SerializeField] Material targetMaterial;
    [SerializeField] Material deployMaterial;

    private MeshRenderer meshRenderer;

	private void Awake() {
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		meshRenderer.enabled = false;
	}

	public void SetSelectable(Tile.SelectionType selectionType) {
		if (selectionType == Tile.SelectionType.NONE || (!FindObjectOfType<TurnSystem>().IsCombat && selectionType != Tile.SelectionType.DEPLOY)) {
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
			case Tile.SelectionType.DEPLOY:
				meshRenderer.material = deployMaterial;
				break;
		}
	}

	public void SetTarget() {
		if (!FindObjectOfType<TurnSystem>().IsCombat) return;
		meshRenderer.enabled = true;
		meshRenderer.material = targetMaterial;
	}
}
