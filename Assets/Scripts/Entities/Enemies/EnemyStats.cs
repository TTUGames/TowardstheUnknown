public class EnemyStats : EntityStats
{
    public int maxMovementPoints = 3;
    private int movementPoints;

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
