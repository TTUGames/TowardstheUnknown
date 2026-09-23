using UnityEngine;

public class DuelMastery : SingleTargetArtifact
{
    private int minDamage = 30;
    private int maxDamage = 30;
    private int armor = 30;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SOURCETILE);
        playerColor = Color.white;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.EPIC;
        attackDuration = 3f;
        cost = 3;
        SetRange(new LineTileSearch(), 1, 1);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (1, 1), (0, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, armor);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ArmorAction(source, armor));
    }
}
