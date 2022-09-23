using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMap : MonoBehaviour
{
    List<GameObject> mapPrefab = new List<GameObject>();
    
    public void LoadMaps()
    {
        mapPrefab[1] = Resources.Load<GameObject>("Prefabs/Map/Map2");
        mapPrefab[2] = Resources.Load<GameObject>("Prefabs/Map/Map1");
        mapPrefab[3] = Resources.Load<GameObject>("Prefabs/Map/Map2");
        mapPrefab[4] = Resources.Load<GameObject>("Prefabs/Map/Map1");
    }
    
    public void StartTransitionToNextMap(int numMap)
    {
        print("Change map");
    }
}
