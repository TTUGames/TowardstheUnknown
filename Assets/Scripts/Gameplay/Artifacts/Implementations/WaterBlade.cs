using UnityEngine;

public class WaterBlade : AoeArtifact
{
    private int minDamage = 20;
    private int maxDamage = 25;
    private int buffDuration = 2;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD);
        playerColor = Color.blue;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.RARE;
        attackDuration = 2.5f;
        cost = 2;
        SetRange(new CircleAttackTS(), 1, 2);
        SetArea(new CircleTileSearch(), 0, 1);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (1, 0), (0, 1), (2, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(buffDuration)));
    }
}
