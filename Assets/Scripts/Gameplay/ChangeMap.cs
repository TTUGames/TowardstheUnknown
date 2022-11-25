using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMap : MonoBehaviour
{
    [SerializeField] private int mapXSize = 13;
    [SerializeField] private int mapZSize = 14;

    [SerializeField] private float baseXPosition = 1.5f;
    [SerializeField] private float baseYPosition = -0.2f;
    [SerializeField] private float baseZPosition = 7.5f;

    [SerializeField] private float transitionTime = 2f;

    private GameObject currentMap;
    private GameObject nextMap;
    private GameObject player;
    private GameObject ui;
    private Dictionary<Direction, GameObject> aMapPrefab = new Dictionary<Direction, GameObject>();

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ui = GameObject.FindGameObjectWithTag("UI");
    }

    private void Start()
    {
        LoadAjdacentRoom();
    }

    /// <summary>
    /// Load all 4 rooms adjacent to the current room
    /// </summary>
    public void LoadAjdacentRoom()
    {
        aMapPrefab.Add(Direction.NORTH, Resources.Load<GameObject>("Prefabs/Maps/Map2_Codir2"));
        aMapPrefab.Add(Direction.SOUTH, Resources.Load<GameObject>("Prefabs/Maps/Map2_Codir2"));
        aMapPrefab.Add(Direction.EAST, Resources.Load<GameObject>("Prefabs/Maps/Map2_Codir2"));
        aMapPrefab.Add(Direction.WEST, Resources.Load<GameObject>("Prefabs/Maps/Map2_Codir2"));
    }

    /// <summary>
    /// Launch a <c>Coroutine</c> to change the map
    /// </summary>
    /// <param name="exitDirection">This is the number of the direction where the exit has been triggered</param>
    public void StartTransitionToNextMap(Direction exitDirection)
    {
        player.GetComponent<PlayerMove>().IsPlaying = false;
        ui.GetComponent<UIFade>().Fade(true);
        StartCoroutine(MoveMapOnSide(exitDirection));
    }

    /// <summary>
    /// Create the new map and slide it at the position of the current map. <br/>
    /// </summary>
    /// <param name="exitDirection">This is the number of the direction where the exit has been triggered</param>
    /// <returns></returns>
    private IEnumerator MoveMapOnSide(Direction exitDirection)
    {
        yield return new WaitForSeconds(transitionTime);
        
        currentMap      = GameObject.FindGameObjectWithTag("Map");
        nextMap         = null;

        Vector3 finalPosCurrentMap = Vector3.zero;
        Vector3 finalPosNextMap    = Vector3.zero;


        switch (exitDirection)
        {
            //Top
            case Direction.NORTH: 
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(0, 0, 0 + mapZSize), Quaternion.identity);

                finalPosCurrentMap = new Vector3(currentMap.transform.position.x, currentMap.transform.position.y, currentMap.transform.position.z - mapZSize - 1);
                finalPosNextMap    = new Vector3(currentMap.transform.position.x, currentMap.transform.position.y, currentMap.transform.position.z);
                break;

            //Right
            case Direction.EAST:
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(0 + mapXSize, 0, 0), Quaternion.identity);
                
                finalPosCurrentMap = new Vector3(currentMap.transform.position.x - mapXSize   , currentMap.transform.position.y, currentMap.transform.position.z);
                finalPosNextMap = currentMap.transform.position;
                break;

            //Bottom
            case Direction.SOUTH:
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(0,0,0 - mapZSize), Quaternion.identity);
                finalPosCurrentMap = new Vector3(currentMap.transform.position.x, currentMap.transform.position.y, currentMap.transform.position.z + mapZSize + 1);
                finalPosNextMap = currentMap.transform.position;
                break;
                
            //Left
            case Direction.WEST:
                nextMap = Instantiate(aMapPrefab[exitDirection], new Vector3(0 - mapXSize, 0, 0), Quaternion.identity);

                finalPosCurrentMap = new Vector3(currentMap.transform.position.x + mapXSize   , currentMap.transform.position.y, currentMap.transform.position.z);
                finalPosNextMap = currentMap.transform.position;
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
        ui.GetComponent<UIFade>().Fade(false);

        Destroy(currentMap); //Not recommended but we're in a thread, it should be fine
        yield return new WaitForEndOfFrame();
        player.GetComponent<PlayerMove>().IsPlaying = true;
        player.GetComponent<PlayerMove>().Init();
        player.GetComponent<PlayerAttack>().Init();
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
    
    private void PlacePlayer(Direction exitDirection)
    {
        GameObject[] aMapChangerTile = GameObject.FindGameObjectsWithTag("MapChangerTile");
        
        for (int i = 0; i < aMapChangerTile.Length; i++)
            if      (aMapChangerTile[i].GetComponent<TransitionTile>().direction == Direction.SOUTH && exitDirection == Direction.NORTH) //From North to South
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x    , 0, aMapChangerTile[i].transform.position.z + 1);
            else if (aMapChangerTile[i].GetComponent<TransitionTile>().direction == Direction.WEST && exitDirection == Direction.EAST) //From East to West
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x + 1, 0, aMapChangerTile[i].transform.position.z);
            else if (aMapChangerTile[i].GetComponent<Tile>().GetComponent<TransitionTile>().direction == Direction.NORTH && exitDirection == Direction.SOUTH) //From South to North
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x    , 0, aMapChangerTile[i].transform.position.z - 1);
            else if (aMapChangerTile[i].GetComponent<Tile>().GetComponent<TransitionTile>().direction == Direction.EAST && exitDirection == Direction.WEST) //From West to East
                player.transform.position = new Vector3(aMapChangerTile[i].transform.position.x - 1, 0, aMapChangerTile[i].transform.position.z);
    }
}
