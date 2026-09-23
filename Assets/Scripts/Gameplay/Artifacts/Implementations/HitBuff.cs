using UnityEngine;

public class HitBuff : SingleTargetArtifact
{
    private int minDamage = 5;
    private int maxDamage = 10;
    private int buffDuration = 1;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD);
        playerColor = Color.red;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;
        cost = 0;
        SetRange(new CircleAttackTS(), 0, 0);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (1, 0), (2, 0));
        targets.Add("Player");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, source, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new AttackUpStatus(buffDuration)));
    }
}
