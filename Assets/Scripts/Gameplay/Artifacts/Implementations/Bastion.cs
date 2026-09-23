using UnityEngine;

public class Bastion : AoeArtifact
{
    private int armor = 60;
    private int buffDuration = 2;
    private int pushDistance = 2;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Color.yellow;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 3f;
        cost = 2;
        SetRange(new CircleAttackTS(), 0, 0);
        SetArea(new CircleTileSearch(), 1, 1);
        maximumUsePerTurn = 1;
        cooldown = 3;
        slots = Shape((0, 0), (0, 1), (1, 0), (1, 1), (2, 0), (2, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, pushDistance, armor, buffDuration);
    }

    protected override void ApplyEffectOnCast(EntityStats source)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(source, new DefenseUpStatus(buffDuration)));
        ActionManager.AddToBottom(new ArmorAction(source, armor));
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
    }
}
