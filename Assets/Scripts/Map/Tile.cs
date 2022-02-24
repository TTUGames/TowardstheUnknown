using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]private bool isTravesable;

    public bool isSelectible = false;
    public bool isCurrent = false; //if player is in that tile
    public bool isTarget = false;

    public List<Tile> adjacencyList = new List<Tile>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isCurrent)
            GetComponent<Renderer>().material.color = Color.yellow;
        else if (isTarget)
            GetComponent<Renderer>().material.color = Color.green;
        else if (isSelectible)
            GetComponent<Renderer>().material.color = Color.red;
        else
            GetComponent<Renderer>().material.color = Color.white;
    }

    public bool getIsTravesable()
    { return isTravesable; }

    public void setIsTraversable(bool state)
    { isTravesable = state; }
}
