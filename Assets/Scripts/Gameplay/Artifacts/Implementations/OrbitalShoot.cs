using UnityEngine;

public class OrbitalShoot : SingleTargetArtifact
{
    private int minDamage = 25;
    private int maxDamage = 30;
    private int pushDistance = 2;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Color.red;
        weapon = WeaponEnum.gun;
        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;
        cost = 4;
        SetRange(new LineTileSearch(), 1, 100);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (0, 1), (0, 2));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, pushDistance);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new MoveTowardsAction(target, source, -pushDistance));
    }
}
