using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPause : MonoBehaviour
{
    public GameObject pause;
    public bool isPaused = false;
    public Animator animator;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                isPaused = false;
                animator.Play("PauseMenuAnimationOff");
            }
            else
            {
                isPaused = true;
                animator.Play("PauseMenuAnimationOn");
            }
        }
    }
}
