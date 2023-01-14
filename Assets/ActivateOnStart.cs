using UnityEngine;

public class DisableReactivate : MonoBehaviour
{
    public GameObject objectToActivate;

    void Start()
    {
        objectToActivate.SetActive(true);
    }
}