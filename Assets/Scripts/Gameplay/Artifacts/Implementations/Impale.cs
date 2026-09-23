using UnityEngine;

public class Impale : SingleTargetArtifact
{
    private int minDamage = 35;
    private int maxDamage = 85;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD);
        playerColor = Color.blue;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;
        cost = 4;
        SetRange(new LineTileSearch(), 1, 1);
        maximumUsePerTurn = 1;
        cooldown = 3;
        slots = Shape((0, 0), (0, 1), (0, 2), (0, 3));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
