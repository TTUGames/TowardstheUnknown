using UnityEngine;

public class Estoc : SingleTargetArtifact
{
    private int minDamage = 25;
    private int maxDamage = 30;
    private int buffDuration = 1;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD, 1f);
        playerColor = Color.white;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;
        cost = 2;
        SetRange(new LineAttackTS(), 1, 2);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (1, 0), (2, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseDownStatus(buffDuration)));
    }
}
