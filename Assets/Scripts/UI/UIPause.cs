using UnityEngine;

public class UIPause : MonoBehaviour
{
    [SerializeField] private GameObject backgroundPause;
    [SerializeField] private GameObject PauseMain;
    [SerializeField] private GameObject PauseOptions;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator backgroundAnimator;
    [SerializeField] private ChangeUI changeUI;
    private PlayerStats playerStats;

    public bool isPaused = false;

    private void Start()
    {
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
    }

    public void ChangeStateOptions()
    {
            if (changeUI.IsInventoryOpened)
            {
                changeUI.ChangeStateInventory();
            }
            else if (PauseOptions.activeSelf)
            {
                BackOptions();
            }
            else
            {
                ToggleOptions(!isPaused);
            }
    }

    public void ToggleOptions(bool state)
    {
        isPaused = state;
        changeUI.UIInformation();
        changeUI.ChangeBlur(state);
        backgroundPause.SetActive(state);
        animator.Play(state ? "PauseMenuAnimationOn" : "PauseMenuAnimationOff");
        backgroundAnimator.Play(state ? "On" : "Off");
        miniMap.SetActive(!state && !inventoryMenu.activeSelf);

        PauseMain.SetActive(state);
        PauseOptions.SetActive(false);
    }

    public void BackOptions()
    {
        PauseOptions.SetActive(false);
        PauseMain.SetActive(true);
    }
}
