using UnityEngine;

public class OpenOptions : MonoBehaviour
{
    public GameObject objectToToggle1;
    public GameObject objectToToggle2;

    public void OnButtonClick()
    {
        objectToToggle1.SetActive(!objectToToggle1.activeSelf);
        objectToToggle2.SetActive(!objectToToggle2.activeSelf);
    }
}
