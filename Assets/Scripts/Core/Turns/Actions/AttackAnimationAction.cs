using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays an attack's animation and VFX, and waits for its duration
/// </summary>
public class AttackAnimationAction : GameAction {
	private readonly GameObject source;
	private readonly Tile targetTile;
	private readonly float duration;
	private readonly string animationState;
	private readonly IEnumerable<VFXInfo> vfxInfos;
	private readonly List<GameObject> vfxs = new List<GameObject>();

	/// <param name="animationState">The animator state played on the source, none if null or empty</param>
	public AttackAnimationAction(GameObject source, Tile targetTile, float duration, string animationState, IEnumerable<VFXInfo> vfxInfos) {
		this.source = source;
		this.targetTile = targetTile;
		this.duration = duration;
		this.animationState = animationState;
		this.vfxInfos = vfxInfos;
	}

	protected override void OnStart() {
		if (!string.IsNullOrEmpty(animationState) && source.TryGetComponent(out Animator animator))
			animator.Play(animationState);
		foreach (VFXInfo vfxInfo in vfxInfos)
			vfxInfo.Play(this, source, targetTile);
		ActionManager.Run(WaitAndEndAttack());
	}

	public void AddVFX(GameObject vfx) {
		vfxs.Add(vfx);
	}

	private IEnumerator WaitAndEndAttack() {
		yield return new WaitForSeconds(duration);
		foreach (GameObject vfx in vfxs)
			if (vfx != null) Object.Destroy(vfx);
		isDone = true;
		//Any attack, enemies' included, ends the player's attack visuals
		PlayerTurn player = GameScene.Player;
		if (player != null) player.playerAttack.EndAttackVisuals();
	}
}
