
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITimer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Timer timer;
    [SerializeField] private TMP_Text timingText;
    private bool hasToBeUpdated = true;

    private void Awake()
    {
        if (timer == null)
            timer = FindObjectOfType<Timer>();
    }

    private void Update()
    {
        if(hasToBeUpdated)
            RefreshUI(); 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hasToBeUpdated = false;
        timingText.text = "FINIR LE TOUR";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hasToBeUpdated = true;
        RefreshUI();
    }

    public void RefreshUI()
    {
        timingText.text = timer.timeRemaining.ToString();
    }
}