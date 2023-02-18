using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowHideObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject objectToShowHide;
    private GameObject targetEntity;
    private ChangeUI changeUI;

    private void Start()
    {
        changeUI = GameObject.Find("UI").GetComponent<ChangeUI>();
        objectToShowHide.SetActive(false);
        targetEntity = transform.parent.gameObject.GetComponent<DisplayStats>().entity;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!changeUI.uIIsOpen)
        {
            AkSoundEngine.PostEvent("HoverTimeline", gameObject);
            objectToShowHide.SetActive(true);
            targetEntity.GetComponent<Outline>().enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!changeUI.uIIsOpen)
        {
            objectToShowHide.SetActive(false);
            targetEntity.GetComponent<Outline>().enabled = false;
        }
    }
}
