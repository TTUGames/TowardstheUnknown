using UnityEngine;

public class PrecisionShoot : SingleTargetArtifact
{
    private int minDamage = 20;
    private int maxDamage = 30;

    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/PrecisionShootBullet", VFXInfo.Target.GUN, 0.6f));
        vfxInfos.Add(new VFXInfo("VFX/PrecisionShootMark", VFXInfo.Target.TARGETTILE));
        playerColor = Color.blue;
        weapon = WeaponEnum.gun;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 2f;
        cost = 3;
        SetRange(new CircleAttackTS(), 3, 5);
        maximumUsePerTurn = 2;
        slots = Shape((0, 0), (0, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
