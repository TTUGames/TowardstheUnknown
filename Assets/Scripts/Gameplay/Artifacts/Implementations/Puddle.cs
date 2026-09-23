public class Puddle : AoeArtifact
{
    private int minDamage = 10;
    private int maxDamage = 20;
    private int buffDuration = 2;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Purple;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 4f;
        cost = 3;
        SetRange(new CircleAttackTS(), 1, 4);
        SetArea(new CircleTileSearch(), 0, 2);
        maximumUsePerTurn = 1;
        cooldown = 3;
        slots = Shape((0, 0), (1, 0), (0, 1), (1, 1));
        targets.Add("Enemy");
        effectDescription = string.Format(effectDescription, minDamage, maxDamage, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new DefenseDownStatus(buffDuration)));
        ActionManager.AddToBottom(new DamageAction(source, target, minDamage, maxDamage));
    }
}
