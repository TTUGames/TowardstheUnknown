using UnityEngine;

public class WithoutFear : SingleTargetArtifact
{
    private int minDamage = 25;
    private int maxDamage = 35;
    private int pushDistance = 5;
    private int buffDuration = 1;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Color.yellow;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.EPIC;
        attackDuration = 2f;
        cost = 2;
        SetRange(new RushTS(), 2, 5);
        maximumUsePerTurn = 2;
        slots = Shape((0, 0), (1, 0), (2, 0), (3, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(source, target, pushDistance));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseDownStatus(buffDuration)));
    }
}
