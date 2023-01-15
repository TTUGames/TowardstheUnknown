using UnityEngine;
using UnityEngine.UI;

public class ResetSettings : MonoBehaviour
{
    public ChangeContrast changeContrastScript;
    public ChangeLuminosity changeLuminosityScript;

    public void ResetSettingsValue()
    {
        changeContrastScript.slider.value = 0;
        changeContrastScript.UpdateContrast(0);
        changeLuminosityScript.slider.value = 0;
        changeLuminosityScript.UpdateLuminosity(0);
    }
}
