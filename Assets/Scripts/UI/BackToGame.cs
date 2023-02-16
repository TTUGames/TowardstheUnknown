using UnityEngine;

public class BackToGame : MonoBehaviour
{
    public UIPause uIPause;

    public void GoBackToGame()
    {
        uIPause.ToggleOptions(false);
    }
}
