using UnityEngine;

public class Vampirism : SingleTargetArtifact
{
    private int minDamage = 80;
    private int maxDamage = 90;
    private int healValue = 5;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.RIGHTHAND, 0.3f);
        playerColor = Color.yellow;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 4f;
        cost = 7;
        SetRange(new CircleAttackTS(), 1, 1);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (2, 0), (0, 1), (1, 1), (2, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, healValue);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new HealAction(source, healValue));
    }
}
