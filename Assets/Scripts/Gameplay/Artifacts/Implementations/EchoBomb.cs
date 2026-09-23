using UnityEngine;

public class EchoBomb : AoeArtifact
{
    private int minDamage = 30;
    private int maxDamage = 40;

    protected override void InitValues()
    {
        vfxInfos.Add(new VFXInfo("VFX/" + GetType().Name, VFXInfo.Target.TARGETTILE, 0, Vector3.up * 0.5f));
        playerColor = Color.blue;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.EPIC;
        attackDuration = 3.5f;
        cost = 3;
        SetRange(new CircleAttackTS(), 2, 3);
        SetArea(new CircleTileSearch(), 0, 2);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((0, 0), (1, 0), (0, 1), (1, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
