using UnityEngine;

/// <summary>
/// Activates an object on start, so that its Awake runs even if it is disabled in the scene
/// </summary>
public class ActivateOnStart : MonoBehaviour
{
    public GameObject objectToActivate;

    void Start()
    {
        objectToActivate.SetActive(true);
    }
}
