using UnityEngine;

public class Rush : SingleTargetArtifact
{
    private int minDamage = 20;
    private int maxDamage = 25;
    private int pushDistance = 4;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.BACK);
        playerColor = Color.red;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.EPIC;
        attackDuration = 2f;
        cost = 2;
        SetRange(new RushTS(), 1, 4);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (1, 0), (1, 1), (1, 2), (2, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(source, target, pushDistance));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
