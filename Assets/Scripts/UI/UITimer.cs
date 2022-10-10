using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour
{
    [SerializeField] private Timer timer;

    private void Update()
    {
        RefreshUI(); 
    }

    public void RefreshUI()
    {
        GetComponent<Text>().text = timer.timeRemaining.ToString();
    }
}
