using System.Collections;
using UnityEngine;

public class TileOverlay : MonoBehaviour
{
    [SerializeField] Material attackMaterial;
    [SerializeField] Material movementMaterial;
    [SerializeField] Material targetMaterial;
    [SerializeField] Material deployMaterial;
    [SerializeField, Tooltip("The tiles a hovered enemy can hit this turn")] Material threatMaterial;

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

	public void SetThreat() {
		meshRenderer.enabled = true;
		meshRenderer.sharedMaterial = threatMaterial;
	}

	public void SetTarget() {
		meshRenderer.enabled = true;
		meshRenderer.sharedMaterial = targetMaterial;
	}

	// Shown, hidden, shown, in real seconds, then the tile paints itself again
	private static readonly float[] blinkSteps = { 0.08f, 0.06f, 0.14f };
	private Coroutine blinking;

	/// <summary>
	/// While it blinks, the tile's paint waits: the hover repaints the tile under the pointer every frame
	/// </summary>
	public bool IsBlinking => blinking != null;

	// A room left stops the coroutines: the tile must paint again when it comes back
	private void OnDisable() => blinking = null;

	/// <summary>
	/// Blinks the threat material twice: a click on the tile was refused
	/// </summary>
	public void BlinkRefused(Tile tile) {
		if (blinking != null) StopCoroutine(blinking);
		blinking = StartCoroutine(Blink(tile));
	}

	private IEnumerator Blink(Tile tile) {
		for (int i = 0; i < blinkSteps.Length; i++) {
			meshRenderer.enabled = i % 2 == 0;
			meshRenderer.sharedMaterial = threatMaterial;
			yield return new WaitForSecondsRealtime(blinkSteps[i]);
		}
		blinking = null;
		tile.Paint();
	}
}
