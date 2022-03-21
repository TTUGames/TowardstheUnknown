using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : Artifact
{
    // Start is called before the first frame update
    void Start()
    {
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
}
