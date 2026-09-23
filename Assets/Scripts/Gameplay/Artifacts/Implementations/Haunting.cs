public class Haunting : SingleTargetArtifact
{
    private int minDamage = 10;
    private int maxDamage = 20;
    private int debuffDuration = 2;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Purple;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 2f;
        cost = 1;
        SetRange(new CircleAttackTS(), 1, 2);
        maximumUsePerTurn = 1;
        cooldown = 2;
        slots = Shape((1, 0), (0, 1), (1, 1), (2, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, debuffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(debuffDuration)));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
