using UnityEngine;

public class CriticalShot : SingleTargetArtifact
{
    private int minDamage = 60;
    private int maxDamage = 70;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.GUN);
        playerColor = Color.yellow;
        weapon = WeaponEnum.gun;
        rarity = ArtifactRarity.LEGENDARY;
        attackDuration = 5f;
        cost = 3;
        SetRange(new LineAttackTS(), 2, 7);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (1, 0), (2, 0), (3, 0), (4, 0));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
