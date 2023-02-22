using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InfoEntity : MonoBehaviour
{
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
            infoEntityPrefab.transform.position = cam.WorldToScreenPoint(transform.position);
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
