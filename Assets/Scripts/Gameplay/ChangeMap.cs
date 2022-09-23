using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMap : MonoBehaviour
{
    [SerializeField] private int mapXSize = 13;
    [SerializeField] private int mapZSize = 14;

    [SerializeField] private float baseXPosition = 1.5f;
    [SerializeField] private float baseYPosition = 0.2f;
    [SerializeField] private float baseZPosition = 7.5f;

    [SerializeField] private float transitionTime = 2f;

    private GameObject player;
    private GameObject[] aMapPrefab = new GameObject[4];

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        LoadMaps();
    }

    public void LoadMaps()
    {
        aMapPrefab[0] = Resources.Load<GameObject>("Prefabs/Maps/Map_2");
        aMapPrefab[1] = Resources.Load<GameObject>("Prefabs/Maps/Map_1");
        aMapPrefab[2] = Resources.Load<GameObject>("Prefabs/Maps/Map_2");
        aMapPrefab[3] = Resources.Load<GameObject>("Prefabs/Maps/Map_1");
    }
    
    public void StartTransitionToNextMap(int exitDirection)
    {
        StartCoroutine(MoveMapOnSide(exitDirection));
    }

    private IEnumerator MoveMapOnSide(int exitDirection)
    {
        GameObject currentMap      = GameObject.FindGameObjectWithTag("Map");
        GameObject nextMap         = null;

        Vector3 startPosCurrentMap = currentMap.transform.position;
        Vector3 startPosNextMap    = Vector3.zero;

        Vector3 finalPosCurrentMap = Vector3.zero;
        Vector3 finalPosNextMap    = Vector3.zero;


        switch (exitDirection)
        {
            //Top
            case 0: 
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(baseXPosition, baseYPosition, baseZPosition + mapZSize), Quaternion.identity);
                startPosNextMap = nextMap.transform.position;

                finalPosCurrentMap = new Vector3(currentMap.transform.position.x, 0.2f, currentMap.transform.position.y - mapZSize - 1);
                finalPosNextMap    = new Vector3(nextMap.transform.position.x   , 0.2f, nextMap.transform.position.y    - mapZSize);
                break;

            //Right
            case 1:
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(baseXPosition + mapXSize, baseYPosition, baseZPosition), Quaternion.identity);
                startPosNextMap = nextMap.transform.position;

                finalPosCurrentMap = new Vector3(currentMap.transform.position.x - mapXSize - 1, currentMap.transform.position.y, currentMap.transform.position.z);
                finalPosNextMap    = new Vector3(nextMap.transform.position.x    - mapXSize    , currentMap.transform.position.y, currentMap.transform.position.z);
                break;

            //Bottom
            case 2:
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(baseXPosition, baseYPosition, baseZPosition - mapZSize), Quaternion.identity);
                startPosNextMap = nextMap.transform.position;

                finalPosCurrentMap = new Vector3(currentMap.transform.position.x, currentMap.transform.position.y, currentMap.transform.position.z + mapZSize + 1);
                finalPosNextMap    = new Vector3(nextMap.transform.position.x   , nextMap.transform.position.y   , nextMap.transform.position.z + mapZSize);
                break;

            //Left
            case 3:
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(baseXPosition - mapXSize, baseYPosition, 0), Quaternion.identity);
                startPosNextMap = nextMap.transform.position;

                finalPosCurrentMap = new Vector3(currentMap.transform.position.x + mapXSize + 1, currentMap.transform.position.y, currentMap.transform.position.z);
                finalPosNextMap    = new Vector3 (nextMap.transform.position.x   + mapXSize    , nextMap.transform.position.y   , nextMap.transform.position.z);
                break;
        }
        
        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            currentMap.transform.position = Vector3.Lerp(currentMap.transform.position, finalPosCurrentMap, (elapsedTime / transitionTime));
            nextMap.transform.position    = Vector3.Lerp(nextMap.transform.position, finalPosNextMap, (elapsedTime / transitionTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        player.GetComponent<PlayerMove>().isMapTransitioning = false;
        PlacePlayer(exitDirection);
        Destroy(currentMap);
    }

    private void PlacePlayer(int exitDirection)
    {
        //Tile[] aTile = GameObject.FindGameObjectsWithTag("Tile")[0].transform.position = mapPrefab[numMap].transform.position;
        if (exitDirection == 0)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(5, 0, 0);
        }
        else if (exitDirection == 1)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(5, 0, 0);
        }
        else if (exitDirection == 2)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(5, 0, 0);
        }
        else if (exitDirection == 3)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(5, 0, 0);
        }
    }
}
