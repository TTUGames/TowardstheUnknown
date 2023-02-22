using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSphere : MonoBehaviour
{
    void Start()
    {
        GetComponent<Renderer>().sharedMaterial.SetFloat("AppearProgress__1", -1f);
    }
}
