using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : EntityStats
{
    [SerializeField] public int maxMovementPoints = 3;
    [SerializeField] PlayerInfo playerInfo;
    private InfoEntity infoEntity;
    private int movementPoints;

    public override void Start()
    {
        base.Start();
        infoEntity = GetComponent<InfoEntity>();
        playerInfo = GameObject.Find("UI").GetComponent<PlayerInfo>();
    }

    protected override void OnDamageTaken(int amount)
    {
        infoEntity.OnMouseEnter();
        base.OnDamageTaken(amount);
    }

    protected override void Die()
    {
        if (name.Contains("Kameiko"))
        {
            playerInfo.kameikoKilled++;
        }
        else if (name.Contains("Nanuko"))
        {
            playerInfo.nanukoKilled++;
        }
        else if (name.Contains("Golem"))
        {
            playerInfo.golemKilled++;
        }   

        infoEntity.OnMouseExit();
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
