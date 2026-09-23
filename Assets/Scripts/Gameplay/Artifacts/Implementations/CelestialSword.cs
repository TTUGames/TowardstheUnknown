using UnityEngine;

public class CelestialSword : AoeArtifact
{
    private int minDamage = 40;
    private int maxDamage = 50;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE, 1.7f);
        playerColor = Color.yellow;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 4f;
        cost = 4;
        SetRange(new CircleAttackTS(), 1, 2);
        SetArea(new CircleTileSearch(), 0, 1);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (1, 0), (0, 1), (1, 1), (0, 2), (1, 2));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
