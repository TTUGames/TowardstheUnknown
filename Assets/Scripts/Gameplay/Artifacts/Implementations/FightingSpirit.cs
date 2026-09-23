public class FightingSpirit : SingleTargetArtifact
{
    private int buffDuration = 1;

    protected override void InitValues()
    {
        AddVFX(VFXInfo.Target.TARGETTILE);
        playerColor = Purple;
        weapon = WeaponEnum.none;
        rarity = ArtifactRarity.RARE;
        attackDuration = 5f;
        cost = 0;
        SetRange(new CircleAttackTS(), 0, 0);
        maximumUsePerTurn = 1;
        cooldown = 3;
        slots = Shape((0, 0), (1, 0), (0, 1), (1, 1));
        targets.Add("Player");
        effectDescription = string.Format(effectDescription, buffDuration);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackUpStatus(buffDuration)));
    }
}
