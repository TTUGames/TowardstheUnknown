using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays an attack's animation and VFX, and waits until its impact. An <c>AttackRecoveryAction</c> ends it
/// </summary>
public class AttackAnimationAction : GameAction {
	private readonly GameObject source;
	private readonly Tile targetTile;
	private readonly float impactDelay;
	private readonly string animationState;
	private readonly IEnumerable<VFXInfo> vfxInfos;
	private readonly List<GameObject> vfxs = new List<GameObject>();

	/// <param name="impactDelay">Time before the next actions, the attack's effects, start</param>
	/// <param name="animationState">The animator state played on the source, none if null or empty</param>
	public AttackAnimationAction(GameObject source, Tile targetTile, float impactDelay, string animationState, IEnumerable<VFXInfo> vfxInfos) {
		this.source = source;
		this.targetTile = targetTile;
		this.impactDelay = impactDelay;
		this.animationState = animationState;
		this.vfxInfos = vfxInfos;
	}

	protected override void OnStart() {
		if (!string.IsNullOrEmpty(animationState) && source.TryGetComponent(out Animator animator))
			animator.Play(animationState);
		foreach (VFXInfo vfxInfo in vfxInfos)
			vfxInfo.Play(this, source, targetTile);
		ActionManager.Run(WaitForImpact());
	}

	public void AddVFX(GameObject vfx) {
		vfxs.Add(vfx);
	}

	private IEnumerator WaitForImpact() {
		if (impactDelay > 0) yield return new WaitForSeconds(impactDelay);
		isDone = true;
	}

	/// <summary>
	/// Removes the VFX still playing and ends the player's attack visuals
	/// </summary>
	public void End() {
		foreach (GameObject vfx in vfxs)
			if (vfx != null) Object.Destroy(vfx);
		vfxs.Clear();
		//Any attack, enemies' included, ends the player's attack visuals
		PlayerTurn player = GameScene.Player;
		if (player != null) player.playerAttack.EndAttackVisuals();
	}
}
