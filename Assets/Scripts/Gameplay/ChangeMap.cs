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

    private GameObject currentMap;
    private GameObject nextMap;
    private GameObject player;
    private GameObject[] aMapPrefab = new GameObject[4];

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        LoadAjdacentRoom();
    }

    public void LoadAjdacentRoom()
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
        currentMap      = GameObject.FindGameObjectWithTag("Map");
        nextMap         = null;

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
                finalPosNextMap    = new Vector3(nextMap.transform.position.x   + mapXSize    , nextMap.transform.position.y   , nextMap.transform.position.z);
                break;
        }
        
        RemoveTagsMap();

        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            currentMap.transform.position = Vector3.Lerp(currentMap.transform.position, finalPosCurrentMap, (elapsedTime / transitionTime));
            nextMap.transform.position    = Vector3.Lerp(nextMap.transform.position, finalPosNextMap, (elapsedTime / transitionTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        PlacePlayer(exitDirection);
        
        Destroy(currentMap); //Not recommended but we're in a thread, it should be fine
        player.GetComponent<PlayerMove>().isMapTransitioning = false;
    }

    /// <summary>
    /// Remove tags of Tiles to not access it later when GC is destroying
    /// </summary>
    private void RemoveTagsMap()
    {
        for (int i = 0; i < currentMap.transform.childCount; i++)
            currentMap.transform.GetChild(i).gameObject.tag = "Untagged";
    }
    
    private void PlacePlayer(int exitDirection)
    {
        GameObject[] aMapChangerTile = GameObject.FindGameObjectsWithTag("MapChangerTile");
        
        for (int i = 0; i < aMapChangerTile.Length; i++)
            if      (aMapChangerTile[i].GetComponent<Tile>().numRoomToMove == 2 && exitDirection == 0) //From North to South
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x, aMapChangerTile[i].transform.position.y, aMapChangerTile[i].transform.position.z + 1);
            else if (aMapChangerTile[i].GetComponent<Tile>().numRoomToMove == 3 && exitDirection == 1) //From East to West
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x + 1, aMapChangerTile[i].transform.position.y, aMapChangerTile[i].transform.position.z);
            else if (aMapChangerTile[i].GetComponent<Tile>().numRoomToMove == 0 && exitDirection == 2) //From South to North
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x, aMapChangerTile[i].transform.position.y, aMapChangerTile[i].transform.position.z - 1);
            else if (aMapChangerTile[i].GetComponent<Tile>().numRoomToMove == 1 && exitDirection == 3) //From West to East
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x - 1, aMapChangerTile[i].transform.position.y, aMapChangerTile[i].transform.position.z);
    }
}
