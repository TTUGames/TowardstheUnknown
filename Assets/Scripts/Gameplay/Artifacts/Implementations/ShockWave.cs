using UnityEngine;

public class ShockWave : AoeArtifact
{
    private int minDamage = 25;
    private int maxDamage = 35;
    private int pushDistance = 3;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE, 0.5f);
        playerColor = Color.yellow;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.EPIC;
        attackDuration = 5f;
        cost = 3;
        SetRange(new CircleAttackTS(), 0, 0);
        SetArea(new CircleTileSearch(), 1, 1);
        maximumUsePerTurn = 1;
        slots = Shape((0, 1), (1, 0), (1, 1), (1, 2), (2, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
