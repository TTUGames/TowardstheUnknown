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

    private void PlacePlayer(int numMap)
    {
        //Tile[] aTile = GameObject.FindGameObjectsWithTag("Tile")[0].transform.position = mapPrefab[numMap].transform.position;
        if (numMap == 1)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(0, 0, 0);
        }
        else if (numMap == 2)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(0, 0, 0);
        }
        else if (numMap == 3)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(0, 0, 0);
        }
        else if (numMap == 4)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(0, 0, 0);
        }
    }
}
