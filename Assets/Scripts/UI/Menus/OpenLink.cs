using UnityEngine;

public class OpenLink : MonoBehaviour
{
    public string url;

    public void OnButtonClick()
    {
        Application.OpenURL(url);
    }
}
