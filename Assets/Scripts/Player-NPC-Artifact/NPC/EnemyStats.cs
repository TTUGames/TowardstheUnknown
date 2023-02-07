using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : EntityStats
{
    [SerializeField] private int maxMovementPoints = 3;
    private int movementPoints;

    public override void Start()
    {
        base.Start();
    }

    protected override void Die()
    {
		base.Die();

		SteamAchievements.IncrementStat("entity_killed", 1);
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
