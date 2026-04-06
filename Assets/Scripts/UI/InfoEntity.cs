using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InfoEntity : MonoBehaviour
{
    [SerializeField] private float upOffsetPercentage = 0.5f;
    [SerializeField] private float downOffsetPercentage = 0.5f;
    [SerializeField] private float leftOffsetPercentage = 0.02f;

    private Camera cam;
    private GameObject infoEntityPrefab;
    private TMP_Text infoEntityTMP;
    private TMP_Text nameEntityTMP;
    private string entityName;
    private EnemyStats enemyStats;
    private ChangeUI changeUI;

    public void Start()
    {
        changeUI = GameObject.Find("UI").GetComponent<ChangeUI>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        
        List<TMP_Text> list = new List<TMP_Text>();
        list.AddRange(Resources.FindObjectsOfTypeAll<TMP_Text>());

        infoEntityTMP = list.Find(e => e.name == "InfoEntityTMP");
        nameEntityTMP = list.Find(e => e.name == "NameEntityTMP");
        infoEntityPrefab = infoEntityTMP.transform.parent.gameObject;
        
        enemyStats = gameObject.GetComponent<EnemyStats>();

        entityName = Localization.GetEntityDescription(gameObject.name.Replace("(Clone)", "")).NAME;
    }

    public void OnMouseEnter()
    {
        if (!changeUI.uIIsOpen && enemyStats.currentHealth > 0)
        {
            infoEntityPrefab.SetActive(true);
            Vector3 entityScreenPosition = cam.WorldToScreenPoint(transform.position);
            float screenHeight = Screen.height;
            float screenMiddle = screenHeight / 2f;
            float upOffset = screenHeight * upOffsetPercentage;
            float downOffset = screenHeight * downOffsetPercentage;
            float leftOffset = Screen.width * leftOffsetPercentage;
            if (entityScreenPosition.y > screenMiddle)
            {
                entityScreenPosition.y -= upOffset;
            }
            else
            {
                entityScreenPosition.y += downOffset;
            }
            entityScreenPosition.x -= leftOffset;
            infoEntityPrefab.transform.position = entityScreenPosition;
            nameEntityTMP.text = entityName;
            infoEntityTMP.text = "<color=#e82a65>PV : " + enemyStats.currentHealth + " <color=#ffffff>|<color=#20D15F> PM : " + enemyStats.maxMovementPoints;
        }
        else
        {
            infoEntityPrefab.SetActive(false);        
        }
    }

    public void OnMouseExit()
    {
        infoEntityPrefab.SetActive(false);
    }
}
