using System.Collections;
using UnityEngine;

public class TileOverlay : MonoBehaviour
{
    [SerializeField] Material attackMaterial;
    [SerializeField] Material movementMaterial;
    [SerializeField] Material targetMaterial;
    [SerializeField] Material deployMaterial;
    [SerializeField, Tooltip("The tiles a hovered enemy can hit this turn")] Material threatMaterial;
    [SerializeField, Tooltip("The tiles in an aimed artifact's range that its line of sight can't reach")] Material blockedMaterial;

    private MeshRenderer meshRenderer;
    // The Anniversary material painted: a switch paints it again in the edition shown (EditionMaterials doesn't know the paint)
    private Material painted;

	private void Awake() {
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		meshRenderer.enabled = false;
	}

	// The overlay's materials in the edition shown: they are set at each paint, after the room's materials were swapped
	private void Paint(Material material) {
		painted = material;
		EditionSkin skin = GameAssets.Instance.classicSkin;
		meshRenderer.sharedMaterial = skin != null ? skin.Current(material) : material;
	}

	// A room left misses the switches: it catches up when it comes back
	private void OnEnable() {
		OnEditionChanged(Edition.Current);
		Edition.Changed += OnEditionChanged;
	}

	private void OnEditionChanged(GameEdition edition) {
		if (painted != null) Paint(painted);
	}

	public void SetSelectable(Tile.SelectionType selectionType) {
		if (selectionType == Tile.SelectionType.NONE || (selectionType != Tile.SelectionType.DEPLOY && !TurnSystem.Instance.IsCombat)) {
			meshRenderer.enabled = false;
			return;
		}
		meshRenderer.enabled = true;
		Paint(selectionType switch {
			Tile.SelectionType.ATTACK => attackMaterial,
			Tile.SelectionType.MOVEMENT => movementMaterial,
			_ => deployMaterial,
		});
	}

	/// <summary>
	/// Where an aimed push or dash would leave an entity (MovePreview): the deploy material, apart from the others
	/// </summary>
	public void SetMovePreview() {
		meshRenderer.enabled = true;
		Paint(deployMaterial);
	}

	/// <summary>
	/// In the aimed artifact's range, but out of its line of sight
	/// </summary>
	public void SetOutOfSight() {
		meshRenderer.enabled = true;
		Paint(blockedMaterial);
	}

	public void SetThreat() {
		meshRenderer.enabled = true;
		Paint(threatMaterial);
	}

	public void SetTarget() {
		meshRenderer.enabled = true;
		Paint(targetMaterial);
	}

	// Shown, hidden, shown, in real seconds, then the tile paints itself again
	private static readonly float[] blinkSteps = { 0.08f, 0.06f, 0.14f };
	private Coroutine blinking;

	/// <summary>
	/// While it blinks, the tile's paint waits: the blink paints the tile again when it ends
	/// </summary>
	public bool IsBlinking => blinking != null;

	// A room left stops the coroutines: the tile must paint again when it comes back
	private void OnDisable() {
		blinking = null;
		Edition.Changed -= OnEditionChanged;
	}

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
			Paint(threatMaterial);
			yield return new WaitForSecondsRealtime(blinkSteps[i]);
		}
		blinking = null;
		tile.Paint();
	}
}
