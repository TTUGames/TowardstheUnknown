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
        if (other.CompareTag("Player"))
            TryPickUp();
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.E) && !GameObject.FindGameObjectWithTag("UI").GetComponent<ChangeUI>().GetIsInventoryOpen())
            {
                TryPickUp();
            }
        }
    }

    private void TryPickUp() {
        bool wasPickedUp = false;
        Artifact artifact = (Artifact)Activator.CreateInstance(Type.GetType(artifactName));
        wasPickedUp = TetrisSlot.instanceSlot.addInFirstSpace(artifact); //add to the bag matrix.

        if (wasPickedUp)
            Destroy(this.gameObject);
    }
}