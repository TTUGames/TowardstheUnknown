using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UIPause : MonoBehaviour
{
    public GameObject pause;
    public GameObject backgroundPause;
    public GameObject PauseMain;
    public GameObject PauseOptions;
    public GameObject miniMap;
    public GameObject inventoryMenu;
    public bool isPaused = false;
    public Animator animator;
    public Animator backgroundAnimator;
    public ChangeUI changeUI;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseOptions.activeSelf)
            {
                BackOptions();
            }
            else if (isPaused)
            {
                CloseOptions();
            }
            else
            {
                OpenOptions();
            }
        }
    }

    public void OpenOptions()
    {
        isPaused = true;
        changeUI.ChangeBlur(true);
        backgroundPause.SetActive(true);
        animator.Play("PauseMenuAnimationOn");
        backgroundAnimator.Play("On");
        miniMap.SetActive(false);
    }

    public void CloseOptions()
    {
        isPaused = false;
        changeUI.ChangeBlur(false);
        animator.Play("PauseMenuAnimationOff");
        backgroundPause.SetActive(false);
        if (!inventoryMenu.activeSelf)
        {
            miniMap.SetActive(true);
        }
    }

    public void BackOptions()
    {
        PauseOptions.SetActive(false);
        PauseMain.SetActive(true);
    }
}
