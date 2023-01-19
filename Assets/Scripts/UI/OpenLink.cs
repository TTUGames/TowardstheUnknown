using UnityEngine;
using UnityEngine.UI;

public class OpenLink : MonoBehaviour
{
    public string url;

    public void OnButtonClick()
    {
        Application.OpenURL(url);
    }
}
