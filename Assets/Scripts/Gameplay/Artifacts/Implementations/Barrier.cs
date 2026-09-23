using UnityEngine;

public class Barrier : SingleTargetArtifact
{
    private int armor = 30;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Color.blue;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 3f;
        cost = 3;
        SetRange(new CircleAttackTS(), 0, 0);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (1, 0), (2, 0));
        targets.Add("Player");
        effectDescription = string.Format(effectDescription, armor);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, armor));
    }
}
