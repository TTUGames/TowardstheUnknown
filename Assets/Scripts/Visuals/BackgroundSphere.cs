using UnityEngine;

public class BackgroundSphere : MonoBehaviour
{
    void Start()
    {
        GetComponent<Renderer>().sharedMaterial.SetFloat("AppearProgress__1", -1f);
    }
}
