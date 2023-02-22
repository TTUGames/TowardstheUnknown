using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InfoEntity : MonoBehaviour
{
    [SerializeField] private float upOffset = 15f;
    [SerializeField] private float downOffset = -125f;
    [SerializeField] private float leftOffset = 20f;

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
            if (entityScreenPosition.y > Screen.height / 2)
            {
                entityScreenPosition.y += upOffset;
            }
            else
            {
                entityScreenPosition.y -= downOffset;
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
