using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Vector3 posToGo;
    public string artifactName;
    
    /// <summary>
    /// Tries to pickup the item when the player enters the collision
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            TryPickUp();
    }
    
    /// <summary>
    /// Tries to pickup the item when E is pressed while colliding
    /// </summary>
    /// <param name="other"></param>
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

    /// <summary>
    /// Tries to pickup this item, and destroys it if successful
    /// </summary>
    private void TryPickUp() {
        bool wasPickedUp = false;
        Artifact artifact = (Artifact)Activator.CreateInstance(Type.GetType(artifactName));
        wasPickedUp = TetrisSlot.instanceSlot.addInFirstSpace(artifact); //add to the bag matrix.

        if (wasPickedUp)
            Destroy(this.gameObject);
    }
}