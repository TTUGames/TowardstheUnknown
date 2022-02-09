using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]private bool isTravesable;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool getIsTravesable()
    { return isTravesable; }

    public void setIsTraversable(bool state)
    { isTravesable = state; }
}
