using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : MonoBehaviour
{
    public GameObject pause;
    public GameObject backgroundPause;
    public GameObject PauseMain;
    public GameObject PauseOptions;
    public bool isPaused = false;
    public Animator animator;
    public Animator backgroundAnimator;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseOptions.activeSelf)
            {
                PauseOptions.SetActive(false);
                PauseMain.SetActive(true);
            }
            else if (isPaused)
            {
                isPaused = false;
                animator.Play("PauseMenuAnimationOff");
                //backgroundAnimator.Play("Off");
                backgroundPause.SetActive(false);
            }
            else
            {
                backgroundPause.SetActive(true);
                isPaused = true;
                animator.Play("PauseMenuAnimationOn");
                backgroundAnimator.Play("On");
            }
        }
    }
}
