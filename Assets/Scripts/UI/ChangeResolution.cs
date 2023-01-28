using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChangeResolution : MonoBehaviour
{
    public TextMeshProUGUI output;

    public void SetResolution(int val)
    {
        if (val == 0)
        {
            Screen.SetResolution(1280, 720, true);
        }
        
        if (val == 1)
        {
            Screen.SetResolution(1920, 1080, true);
        }
        
        if (val == 2)
        {
            Screen.SetResolution(2560, 1440, true);
        }

        if (val == 3)
        {
            Screen.SetResolution(3840, 2160, true);
        }

    }
}
