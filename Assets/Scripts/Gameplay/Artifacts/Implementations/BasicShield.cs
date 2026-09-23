using UnityEngine;

public class BasicShield : SingleTargetArtifact
{
    private int armor = 50;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Color.blue;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 3f;
        cost = 2;
        SetRange(new CircleAttackTS(), 0, 0);
        maximumUsePerTurn = 1;
        cooldown = 3;
        slots = Shape((0, 0), (1, 0), (0, 1), (1, 1));
        targets.Add("Player");
        effectDescription = string.Format(effectDescription, armor);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, armor));
    }
}
