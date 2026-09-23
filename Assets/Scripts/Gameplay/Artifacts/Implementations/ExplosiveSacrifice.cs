using UnityEngine;

public class ExplosiveSacrifice : AoeArtifact
{
    private int selfDamage = 40;
    private int minDamage = 75;
    private int maxDamage = 100;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SOURCETILE, 0.5f);
        playerColor = Color.red;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 3.5f;
        cost = 4;
        SetRange(new CircleAttackTS(), 0, 0);
        SetArea(new CircleTileSearch(), 0, 2);
        maximumUsePerTurn = 1;
        cooldown = 3;
        slots = Shape((0, 0), (1, 0), (0, 1), (1, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, selfDamage, minDamage, maxDamage);
    }

    protected override void ApplyEffectOnCast(EntityStats source)
    {
        ActionManager.AddToBottom(new DamageAction(source, source, selfDamage, selfDamage));
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
