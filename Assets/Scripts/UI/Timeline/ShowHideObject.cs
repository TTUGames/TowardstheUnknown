using UnityEngine;
using UnityEngine.EventSystems;

public class ShowHideObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject objectToShowHide;
    private GameObject targetEntity;

    private void Start()
    {
        objectToShowHide.SetActive(false);
        targetEntity = transform.parent.gameObject.GetComponent<DisplayStats>().entity;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GameScene.UI.uIIsOpen)
        {
            AkUnitySoundEngine.PostEvent("HoverTimeline", gameObject);
            objectToShowHide.SetActive(true);
            targetEntity.GetComponent<Outline>().enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!GameScene.UI.uIIsOpen)
        {
            objectToShowHide.SetActive(false);
            targetEntity.GetComponent<Outline>().enabled = false;
        }
    }
}
