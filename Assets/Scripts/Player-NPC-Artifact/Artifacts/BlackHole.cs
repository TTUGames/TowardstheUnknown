using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : Artifact, IArtifact
{
    public BlackHole()
    {
        this.Prefab = (GameObject)Resources.Load("VFX/BlackHole/BlackHole", typeof(GameObject));

        cost = 2;

        distanceMin = 3;
        distanceMax = 5;

        areaOfEffectMin = 2;
        areaOfEffectMax = 2;

        damageMin = 100;
        damageMax = 100;

        maximumUsePerTurn = 1;
        cooldown = 2;
        lootRate = 0.01f;

        sizeX = 2;
        sizeY = 2;
    }

    void IArtifact.Launch(Vector3 position)
    {
        //position.y++;
        Instantiate(this.Prefab, position, Quaternion.identity);
    }

    int IArtifact.GetMaxDistance()
    {
        return distanceMax;
    }

    int IArtifact.GetMinDistance()
    {
        return distanceMin;
    }
}
