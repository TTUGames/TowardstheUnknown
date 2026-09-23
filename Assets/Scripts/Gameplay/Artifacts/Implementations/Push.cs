using UnityEngine;

public class Push : SingleTargetArtifact
{
    private int minDamage = 20;
    private int maxDamage = 30;
    private int pushDistance = 2;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.LEFTHAND);
        playerColor = Color.blue;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 3f;
        cost = 2;
        SetRange(new CircleAttackTS(), 1, 1);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (1, 1), (1, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
