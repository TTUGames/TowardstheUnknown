using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIPause : MonoBehaviour
{
    [SerializeField] private GameObject pause;
    [SerializeField] private GameObject backgroundPause;
    [SerializeField] private GameObject PauseMain;
    [SerializeField] private GameObject PauseOptions;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator backgroundAnimator;
    [SerializeField] private ChangeUI changeUI;

    public bool isPaused = false;

    private void Update()
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
        miniMap.SetActive(!inventoryMenu.activeSelf);
    }

    public void BackOptions()
    {
        PauseOptions.SetActive(false);
        PauseMain.SetActive(true);
    }
}
