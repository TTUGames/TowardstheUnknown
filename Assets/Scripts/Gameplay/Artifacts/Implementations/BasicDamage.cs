using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
    private int minDamage = 25;
    private int maxDamage = 30;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD);
        playerColor = Color.white;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;
        cost = 2;
        SetRange(new CircleAttackTS(), 1, 1);
        maximumUsePerTurn = 2;
        slots = Shape((1, 1), (1, 0), (0, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
