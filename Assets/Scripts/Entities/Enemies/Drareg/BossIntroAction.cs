using UnityEngine;

/// <summary>
/// A boss's entrance before the first turn of its combat: it plays its gesture while the camera turns to it and its name
/// shows (<see cref="GameEvents.BossIntroStarted"/>), and the queue waits for it
/// </summary>
public class BossIntroAction : GameAction
{
    private readonly EntityStats boss;
    private readonly AnimationClip gesture;
    private float remaining;

    public BossIntroAction(EntityStats boss, AnimationClip gesture, float seconds)
    {
        this.boss = boss;
        this.gesture = gesture;
        remaining = seconds;
    }

    protected override void OnStart()
    {
        GameEvents.StartBossIntro(boss, remaining);
        if (gesture != null && boss.TryGetComponent(out EntityAnimator animator)) animator.PlayAttack(gesture);
    }

    public override void Apply()
    {
        remaining -= Time.deltaTime;
        if (remaining > 0 && boss != null && !boss.IsDead) return;
        //What is left of the gesture plays out, cut by a move
        if (boss != null && boss.TryGetComponent(out EntityAnimator animator)) animator.EndAttack();
        isDone = true;
    }
}
