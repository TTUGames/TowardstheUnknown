using UnityEngine;
using UnityEngine.UI;

public class CanvasSwitch : MonoBehaviour
{
    public Button switchButton;
    public Canvas from;
    public Canvas to;

    private void Start()
    {
        switchButton.onClick.AddListener(SwitchCanvas);
    }

    public void SwitchCanvas()
    {
        from.gameObject.SetActive(false);
        to.gameObject.SetActive(true);
    }
}
