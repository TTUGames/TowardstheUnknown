using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The end of run screen (Assets/UI/Menus/Results.uxml)
/// </summary>
public class Results : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private PlayerInfo playerInfo;
    private ChangeUI changeUI;
    private VisualElement screen;

    public bool IsShown => screen != null && screen.ClassListContains("open");

    void Awake()
    {
        playerInfo = GetComponent<PlayerInfo>();
        changeUI = GetComponent<ChangeUI>();
    }

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        screen = document.rootVisualElement.Q("Results");
        MenuScreen.Setup(screen, gameObject);
        screen.Q<Button>("Restart").clicked += GameFlow.StartRun;
        screen.Q<Button>("MainMenu").clicked += GameFlow.LoadMainMenu;
    }

    private void OnEnable()
    {
        GameEvents.RunEnded += DisplayResultCanvas;
    }

    private void OnDisable()
    {
        GameEvents.RunEnded -= DisplayResultCanvas;
    }

    private void DisplayResultCanvas(bool isVictory)
    {
        screen.AddToClassList("open");

        screen.Q<Label>("Score").text = string.Format(Localization.UI("EndScreenScore"), playerInfo.score.ToString());
        if (playerInfo.score >= 50000)
            SteamAchievements.SetAchievement("ACH_MAXSCORE");

        Label message = screen.Q<Label>("Message");
        message.text = Localization.UI(isVictory ? "EndScreenVictory" : "EndScreenDefeat");
        message.EnableInClassList("victory", isVictory);
        message.EnableInClassList("defeat", !isVictory);
    }
}
