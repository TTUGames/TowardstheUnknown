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
		if (selectionType == Tile.SelectionType.NONE || (selectionType != Tile.SelectionType.DEPLOY && !TurnSystem.Instance.IsCombat)) {
			meshRenderer.enabled = false;
			return;
		}
		meshRenderer.enabled = true;
		meshRenderer.sharedMaterial = selectionType switch {
			Tile.SelectionType.ATTACK => attackMaterial,
			Tile.SelectionType.MOVEMENT => movementMaterial,
			_ => deployMaterial,
		};
	}

	public void SetTarget() {
		meshRenderer.enabled = true;
		meshRenderer.sharedMaterial = targetMaterial;
	}
}
