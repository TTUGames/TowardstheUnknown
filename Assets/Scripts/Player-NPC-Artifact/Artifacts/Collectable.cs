using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    // enable pickingUp items (K)
    public Vector3 posToGo;
    public Artifact itemTetris;

    private void OnTriggerStay3D(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                bool wasPickedUpTestris = false;
                wasPickedUpTestris = TetrisSlot.instanceSlot.addInFirstSpace(itemTetris); //add to the bag matrix.
                if (wasPickedUpTestris) // took
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

}
