using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArtifact
{
    public void Launch();

    public int GetMaxDistance();
    public int GetMinDistance();
}
