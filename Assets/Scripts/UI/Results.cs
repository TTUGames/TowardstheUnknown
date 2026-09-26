using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The end of run screen (Assets/UI/Menus/Results.uxml)
/// </summary>
public class Results : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private UISounds sounds;
    private VisualElement screen;

    public bool IsShown => screen != null && screen.ClassListContains("open");

    // The UIDocument builds its tree in OnEnable, before any Start
    private void Start()
    {
        screen = document.rootVisualElement.Q("Results");
        MenuScreen.Setup(screen, gameObject, sounds);
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

        screen.Q<Label>("Score").text = string.Format(Localization.UI("EndScreenScore"), GameScene.Run.Score.ToString());

        Label message = screen.Q<Label>("Message");
        message.text = Localization.UI(isVictory ? "EndScreenVictory" : "EndScreenDefeat");
        message.EnableInClassList("victory", isVictory);
        message.EnableInClassList("defeat", !isVictory);
        GameScene.UI.NotifyMenuChanged();
    }
}
