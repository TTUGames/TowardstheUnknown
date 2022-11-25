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
        //TODO mettre un ptit logo E au dessus du perso ou un truc du genre
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.E) && !GameObject.FindGameObjectWithTag("UI").GetComponent<ChangeUIState>().GetIsInventoryOpen())
            {
                bool wasPickedUp = false;
                Artifact artifact = (Artifact)Activator.CreateInstance(Type.GetType(artifactName));
                wasPickedUp = TetrisSlot.instanceSlot.addInFirstSpace(artifact); //add to the bag matrix.
                
                if (wasPickedUp)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}