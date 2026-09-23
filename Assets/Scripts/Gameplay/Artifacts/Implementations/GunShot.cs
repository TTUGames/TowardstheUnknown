using UnityEngine;

public class Gunshot : SingleTargetArtifact
{
    private int minDamage = 30;
    private int maxDamage = 40;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.GUN);
        playerColor = Color.yellow;
        weapon = WeaponEnum.gun;
        rarity = ArtifactRarity.EPIC;
        attackDuration = 2.5f;
        cost = 2;
        SetRange(new CircleAttackTS(), 1, 3);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (0, 1), (1, 1), (2, 1), (1, 2));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
