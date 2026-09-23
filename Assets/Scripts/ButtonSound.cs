using UnityEngine;
using UnityEngine.EventSystems;
public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AkUnitySoundEngine.PostEvent("Button_Hover", gameObject);
    }

    public void SoundOnClick()
    {
        AkUnitySoundEngine.PostEvent("Button_Click", gameObject);
    }
}
