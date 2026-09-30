using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays an attack's animation, VFX and sound, and waits until its impact: its strike, or the arrival of its projectile. An <c>AttackRecoveryAction</c> ends it
/// </summary>
public class AttackAnimationAction : GameAction {
	private readonly GameObject source;
	private readonly Tile targetTile;
	private readonly float impactDelay;
	private readonly AbilityData data;
	private readonly List<(GameObject vfx, float playedAt)> vfxs = new List<(GameObject, float)>();
	private float startTime;
	private bool vfxReleased;

	/// <summary>
	/// Seconds from the start of the attack to its impact, known once the action is done
	/// </summary>
	public float ImpactTime { get; private set; }

	/// <summary>
	/// The attack's timing: the clip, the impact and the VFX delays follow it
	/// </summary>
	public AttackClock Clock { get; private set; } = AttackClock.Linear;

	/// <summary>
	/// Seconds the next actions wait after the impact, as the attack's timing sets them, or negative for the rest of its duration
	/// </summary>
	public float Recovery => Clock != AttackClock.Linear && data.timing.recovery > 0 ? data.timing.recovery : -1;

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
		Clock = data.Clock();
		if (source.TryGetComponent(out EntityAnimator animator))
			animator.PlayAttack(data.animationClip, data.animationSpeed, data.followUpClip, Clock, data.legs);
		if (data.SwingsBlade && source.TryGetComponent(out WeaponTrail trail)) {
			(float start, float end) = data.SwingWindow(Clock);
			trail.Swing(start, end, data.AttackColor);
		}
		foreach (VFXInfo vfxInfo in data.vfx)
			vfxInfo.Play(this, source, targetTile);
		ActionManager.Run(PlaySound());
		ActionManager.Run(WaitForImpact());
	}

	private IEnumerator PlaySound() {
		float delay = data.SoundDelay;
		if (delay > 0) yield return new WaitForSeconds(delay);
		if (source != null) data.sound.Post(source);
	}

	public void AddVFX(GameObject vfx) {
		//A VFX delayed past the attack's VFX duration would never be removed
		if (vfxReleased) Release(vfx, Time.time);
		else vfxs.Add((vfx, Time.time));
	}

	//A VFX leaving a mark (VFXLifetime) stays until its mark has faded, past the ability's VFX duration
	private static void Release(GameObject vfx, float playedAt) {
		float until = vfx != null && vfx.TryGetComponent(out VFXLifetime lifetime) ? playedAt + lifetime.seconds : 0;
		if (until > Time.time) VFXPool.Release(vfx, until - Time.time);
		else VFXPool.Release(vfx);
	}

	private IEnumerator WaitForImpact() {
		float impact = Clock.EventTime(impactDelay);
		if (impact > 0) yield return new WaitForSeconds(impact);
		//The effects wait for the projectile: the farther the target, the later they apply
		if (data.projectile.Prefab != null && source != null)
			yield return data.projectile.Fly(this, source.GetComponent<TacticsMove>(), targetTile);
		ImpactTime = Time.time - startTime;
		isDone = true;
	}

	/// <summary>
	/// Lets the VFX play until the ability's VFX duration since the start of the attack, then removes them
	/// </summary>
	public void ReleaseVFXLater() {
		ActionManager.Run(ReleaseVFXAt(startTime + data.vfxDuration));
	}

	private IEnumerator ReleaseVFXAt(float time) {
		if (time > Time.time) yield return new WaitForSeconds(time - Time.time);
		foreach ((GameObject vfx, float playedAt) in vfxs)
			Release(vfx, playedAt);
		vfxs.Clear();
		vfxReleased = true;
	}

	/// <summary>
	/// Ends the player's attack visuals and lets a move cut the rest of the caster's clip; the VFX play out their duration
	/// </summary>
	public void End() {
		ReleaseVFXLater();
		if (source != null && source.TryGetComponent(out EntityAnimator animator)) animator.EndAttack();
		//Any attack, enemies' included, ends the player's attack visuals
		PlayerTurn player = GameScene.Player;
		if (player != null) player.playerAttack.EndAttackVisuals();
	}
}
