using UnityEngine;
using UnityEngine.EventSystems;

public class HoverActivator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject objectToActivate;

    public void OnPointerEnter(PointerEventData eventData)
    {
        objectToActivate.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        objectToActivate.SetActive(false);
    }
}
