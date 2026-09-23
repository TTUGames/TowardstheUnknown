using UnityEngine;

public class SlashAttack : AoeArtifact
{
    private int minDamage = 20;
    private int maxDamage = 30;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD, 0.2f);
        playerColor = Color.red;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 2.5f;
        cost = 3;
        SetRange(new CircleAttackTS(), 1, 1);
        SetArea(new CircleTileSearch(), 0, 1);
        maximumUsePerTurn = 2;
        slots = Shape((0, 0), (1, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
