using UnityEngine;

public class OffensiveFluid : SingleTargetArtifact
{
    private int buffDuration = 2;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Color.red;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;
        cost = 2;
        SetRange(new CircleAttackTS(), 0, 0);
        maximumUsePerTurn = 1;
        cooldown = 3;
        slots = Shape((0, 0), (0, 1), (1, 0));
        targets.Add("Player");
        effectDescription = string.Format(effectDescription, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(buffDuration)));
    }
}
