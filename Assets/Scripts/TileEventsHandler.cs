using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEventsHandler : MonoBehaviour
{
    [SerializeField] PlayerMoves playerMoves;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseUpAsButton()
    {
        Debug.Log("oui");
        playerMoves.Move(transform.position);
    }
}
