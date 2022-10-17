using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    // enable pickingUp items (E)
    public Vector3 posToGo;
    public Artifact itemTetris;

    private void OnTriggerEnter(Collider other)
    {
        print("here");
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("here");
            if (Input.GetKey(KeyCode.E))
            {
                print("took");
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
