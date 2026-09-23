using UnityEngine;

public class ProtectiveEnvelope : SingleTargetArtifact
{
    private int buffDuration = 1;
    private int armor = 40;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Color.blue;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;
        cost = 3;
        SetRange(new CircleAttackTS(), 0, 0);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (0, 1), (0, 2), (1, 0), (1, 1));
        targets.Add("Player");
        effectDescription = string.Format(effectDescription, armor, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ArmorAction(source, armor));
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseUpStatus(buffDuration)));
    }
}
