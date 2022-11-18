using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTargetting
{
    private int distance;

    public AbstractTargetting(int distance) {
        this.distance = distance;
	}

    /// <summary>
    /// Finds and returns a target
    /// </summary>
    /// <param name="source">The entity using this Targetting</param>
    /// <returns></returns>
    public abstract EntityStats GetTarget(EntityStats source);

    public int GetDistance() {
        return distance;
	}
}
