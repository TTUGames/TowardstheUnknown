using UnityEngine;

public class BackToGame : MonoBehaviour
{
    public UIPause Gameplay;

    public void GoBackToGame()
    {
        Gameplay.isPaused = false;
        Gameplay.animator.Play("PauseMenuAnimationOff");
    }
}
