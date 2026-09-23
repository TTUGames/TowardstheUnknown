using UnityEngine;
using TMPro;

public class InfoEntity : MonoBehaviour
{
    [SerializeField] private float upOffsetPercentage = 0.5f;
    [SerializeField] private float downOffsetPercentage = 0.5f;
    [SerializeField] private float leftOffsetPercentage = 0.02f;

    //Shared by all entities, the panel being unique
    private static GameObject infoEntityPanel;
    private static TMP_Text infoEntityTMP;
    private static TMP_Text nameEntityTMP;

    private Camera cam;
    private string entityName;
    private EnemyStats enemyStats;
    private ChangeUI changeUI;

    public void Start()
    {
        changeUI = GameObject.Find("UI").GetComponent<ChangeUI>();
        cam = Camera.main;

        if (infoEntityPanel == null)
        {
            foreach (TMP_Text text in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
            {
                if (text.name == "InfoEntityTMP") infoEntityTMP = text;
                else if (text.name == "NameEntityTMP") nameEntityTMP = text;
            }
            infoEntityPanel = infoEntityTMP.transform.parent.gameObject;
        }

        enemyStats = GetComponent<EnemyStats>();
        entityName = Localization.GetEntityDescription(gameObject.name.Replace("(Clone)", "")).NAME;
    }

    public void OnMouseEnter()
    {
        if (changeUI.uIIsOpen || enemyStats.currentHealth <= 0)
        {
            infoEntityPanel.SetActive(false);
            return;
        }

        infoEntityPanel.SetActive(true);
        Vector3 entityScreenPosition = cam.WorldToScreenPoint(transform.position);
        if (entityScreenPosition.y > Screen.height / 2f)
            entityScreenPosition.y -= Screen.height * upOffsetPercentage;
        else
            entityScreenPosition.y += Screen.height * downOffsetPercentage;
        entityScreenPosition.x -= Screen.width * leftOffsetPercentage;
        infoEntityPanel.transform.position = entityScreenPosition;
        nameEntityTMP.text = entityName;
        infoEntityTMP.text = "<color=#e82a65>PV : " + enemyStats.currentHealth + " <color=#ffffff>|<color=#20D15F> PM : " + enemyStats.maxMovementPoints;
    }

    public void OnMouseExit()
    {
        infoEntityPanel.SetActive(false);
    }
}
