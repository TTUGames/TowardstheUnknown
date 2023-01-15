using UnityEngine;

public class ActivateOnStart : MonoBehaviour
{
    public GameObject objectToActivate;

    void Start()
    {
        objectToActivate.SetActive(true);
    }
}