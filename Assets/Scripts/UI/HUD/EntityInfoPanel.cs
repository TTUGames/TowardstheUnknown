using TMPro;
using UnityEngine;

/// <summary>
/// The panel showing the name and stats of the hovered enemy, shared by all of them
/// </summary>
public class EntityInfoPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text infoText;

    public void Show(Vector3 screenPosition, string entityName, string info)
    {
        gameObject.SetActive(true);
        transform.position = screenPosition;
        nameText.text = entityName;
        infoText.text = info;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
