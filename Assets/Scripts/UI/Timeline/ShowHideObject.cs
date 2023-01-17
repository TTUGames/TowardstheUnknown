using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowHideObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject objectToShowHide;

    private void Start()
    {
        objectToShowHide.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        objectToShowHide.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        objectToShowHide.SetActive(false);
    }
}
