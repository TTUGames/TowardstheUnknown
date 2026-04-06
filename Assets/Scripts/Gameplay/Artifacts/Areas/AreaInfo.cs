using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaInfo
{
    public int minRange;
    public int maxRange;
    public AreaType type;

    public AreaInfo(int minRange = 0, int maxRange = 0, AreaType type = AreaType.CIRCLE) {
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.type = type;
	}
}
