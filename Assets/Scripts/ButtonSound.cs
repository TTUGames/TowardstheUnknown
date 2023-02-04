using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ButtonSound : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AkSoundEngine.PostEvent("Button_Hover", gameObject);
    }

    public void SoundOnClick()
    {
        AkSoundEngine.PostEvent("Button_Click", gameObject);
    }
}
