using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays an attack's animation and VFX, and waits until its impact: its strike, or the arrival of its projectile. An <c>AttackRecoveryAction</c> ends it
/// </summary>
public class AttackAnimationAction : GameAction {
	private readonly GameObject source;
	private readonly Tile targetTile;
	private readonly float impactDelay;
	private readonly AbilityData data;
	private readonly List<GameObject> vfxs = new List<GameObject>();
	private float startTime;

	/// <summary>
	/// Seconds from the start of the attack to its impact, known once the action is done
	/// </summary>
	public float ImpactTime { get; private set; }

	/// <param name="impactDelay">Time before the strike: the attack's effects, or its projectile, start</param>
	/// <param name="data">The ability, whose clips and VFX play</param>
	public AttackAnimationAction(GameObject source, Tile targetTile, float impactDelay, AbilityData data) {
		this.source = source;
		this.targetTile = targetTile;
		this.impactDelay = impactDelay;
		this.data = data;
	}

	protected override void OnStart() {
		startTime = Time.time;
		if (source.TryGetComponent(out EntityAnimator animator))
			animator.PlayAttack(data.animationClip, data.animationSpeed, data.followUpClip);
		foreach (VFXInfo vfxInfo in data.vfx)
			vfxInfo.Play(this, source, targetTile);
		ActionManager.Run(WaitForImpact());
	}

	public void AddVFX(GameObject vfx) {
		vfxs.Add(vfx);
	}

	private IEnumerator WaitForImpact() {
		if (impactDelay > 0) yield return new WaitForSeconds(impactDelay);
		//The effects wait for the projectile: the farther the target, the later they apply
		if (data.projectile.Prefab != null && source != null)
			yield return data.projectile.Fly(this, source.GetComponent<TacticsMove>(), targetTile);
		ImpactTime = Time.time - startTime;
		isDone = true;
	}

	/// <summary>
	/// Removes the VFX still playing
	/// </summary>
	public void ReleaseVFX() {
		foreach (GameObject vfx in vfxs)
			VFXPool.Release(vfx);
		vfxs.Clear();
	}

	/// <summary>
	/// Removes the VFX still playing and ends the player's attack visuals
	/// </summary>
	public void End() {
		ReleaseVFX();
		//Any attack, enemies' included, ends the player's attack visuals
		PlayerTurn player = GameScene.Player;
		if (player != null) player.playerAttack.EndAttackVisuals();
	}
}
