public class EnemyStats : EntityStats
{
    public int maxMovementPoints = 3;
    private InfoEntity infoEntity;
    private int movementPoints;

    public override void Start()
    {
        base.Start();
        infoEntity = GetComponent<InfoEntity>();
    }

    protected override void OnDamageTaken(int amount)
    {
        infoEntity.Refresh();
    }

    protected override void Die()
    {
        infoEntity.Refresh();
        base.Die();
    }

    public override int GetMovementDistance()
    {
        return movementPoints;
    }

    public override void UseMovement(int distance)
    {
        movementPoints -= distance;
    }

    public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        movementPoints = maxMovementPoints;
    }
}
