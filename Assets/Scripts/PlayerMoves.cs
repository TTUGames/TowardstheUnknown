using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoves : MonoBehaviour
{
    [SerializeField] private float speed;
    private NavMeshAgent Agent;

    private Vector3 finalDestination;
    private Vector3 midDestination;

    // Start is called before the first frame update
    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        finalDestination = new Vector3(0, 0, 0);   
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(midDestination);
        if (transform.position == midDestination)
            ;
    }

    public void Move(Vector3 destination)
    {
        Agent.destination = finalDestination;
    }
}
