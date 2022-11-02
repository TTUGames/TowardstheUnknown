using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Vector3 posToGo;
    [SerializeField] private string artifactName;
    
    private void OnTriggerEnter(Collider other)
    {
        print("here");
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                print("took");
                bool wasPickedUp = false;
                Artifact artifact = (Artifact)System.Activator.CreateInstance(Type.GetType(artifactName));
                wasPickedUp = TetrisSlot.instanceSlot.addInFirstSpace(artifact); //add to the bag matrix.
                if (wasPickedUp) // took
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}