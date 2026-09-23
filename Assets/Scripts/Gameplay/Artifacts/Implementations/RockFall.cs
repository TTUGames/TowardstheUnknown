using UnityEngine;

public class RockFall : SingleTargetArtifact
{
    private int minDamage = 10;
    private int maxDamage = 15;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE, 0.5f);
        playerColor = Color.yellow;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 3.5f;
        cost = 1;
        SetRange(new CircleAttackTS(), 2, 5);
        maximumUsePerTurn = 2;
        slots = Shape((0, 0), (1, 1), (1, 0), (2, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
