using UnityEngine;

public class VFXTool : MonoBehaviour
{
    public GameObject VFXPrefab;
    public float VFXDelay = 0f;
    [Space]
    public string animationName; 
    public float animationDelay = 0f;

    void Update()
    {
        if (Input.GetKeyDown("v"))
        {
            Invoke("StartAnimation", animationDelay);
            Invoke("StartVFX", VFXDelay);
        }
    }
    void StartVFX()
    {
        Instantiate(VFXPrefab);
    }
    void StartAnimation()
    {
        GetComponent<Animator>().Play(animationName);
    }
}
