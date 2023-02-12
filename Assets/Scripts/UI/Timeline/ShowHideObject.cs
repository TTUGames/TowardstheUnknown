using UnityEngine;
using UnityEngine.UI;
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
        AkSoundEngine.PostEvent("HoverTimeline", gameObject);
        objectToShowHide.SetActive(true);
        targetEntity.GetComponent<Outline>().enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        objectToShowHide.SetActive(false);
        targetEntity.GetComponent<Outline>().enabled = false;
    }
}
