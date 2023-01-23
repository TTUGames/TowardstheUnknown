using UnityEngine;
using TMPro;

public class LocalizedUITextSetter : MonoBehaviour
{
    [SerializeField] private string textID;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = Localization.GetUIString(textID).TEXT;
    }
}
