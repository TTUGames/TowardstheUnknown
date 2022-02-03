using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoves : MonoBehaviour
{
    [SerializeField] float speed;

    private Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        destination = new Vector3(0, 0, 0);   
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public void Move(Vector3 destination)
    {
        this.destination = destination;
    }
}
