using UnityEngine;
using UnityEngine.UI;

public class ShowHideObject : MonoBehaviour
{
    public GameObject objectToShowHide;

    private void Start()
    {
        objectToShowHide.SetActive(false);
    }

    public void OnButtonClick()
    {
        objectToShowHide.SetActive(!objectToShowHide.activeSelf);
    }
}
