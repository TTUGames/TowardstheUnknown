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
        ChangeBlur(true);
        backgroundPause.SetActive(true);
        isPaused = true;
        animator.Play("PauseMenuAnimationOn");
        backgroundAnimator.Play("On");
        miniMap.SetActive(false);
    }

    public void CloseOptions()
    {
        ChangeBlur(false);
        isPaused = false;
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

    public void ChangeBlur(bool state)
    {
        DepthOfField dof = new DepthOfField();
        try
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Volume>().profile.TryGet(out dof);
            dof.active = state;
        }
        catch (Exception e)
        {
            print("Global volume not found");
        }
    }


}
