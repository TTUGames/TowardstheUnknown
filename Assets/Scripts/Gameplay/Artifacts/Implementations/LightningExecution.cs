using UnityEngine;

public class LightningExecution : SingleTargetArtifact
{
    private int minDamage = 40;
    private int maxDamage = 50;
    private int buffDuration = 1;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD);
        playerColor = Color.yellow;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.RARE;
        attackDuration = 3f;
        cost = 4;
        SetRange(new CircleAttackTS(), 1, 1);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (1, 0), (1, 1), (2, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
        ActionManager.AddToBottom(new ApplyStatusAction(source, new AttackUpStatus(buffDuration)));
    }
}
