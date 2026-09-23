public class Strike : SingleTargetArtifact
{
    private int minDamage = 15;
    private int maxDamage = 25;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.SWORD, 0.2f);
        playerColor = Purple;
        weapon = WeaponEnum.sword;
        rarity = ArtifactRarity.COMMON;
        attackDuration = 5f;
        cost = 1;
        SetRange(new CircleAttackTS(), 1, 1);
        maximumUsePerTurn = 1;
        slots = Shape((0, 0), (0, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
