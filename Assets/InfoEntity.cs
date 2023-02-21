using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InfoEntity : MonoBehaviour
{
    private Camera cam;
    public GameObject infoEntityPrefab;
    private TMP_Text textMeshPro;
    private EntityStats entityStats;
    private string entityName;
    private PlayerStats playerStats;
    private ChangeUI changeUI;

    public void Start()
    {
        changeUI = GameObject.Find("UI").GetComponent<ChangeUI>();
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        
        List<TMP_Text> list = new List<TMP_Text>();
        list.AddRange(Resources.FindObjectsOfTypeAll<TMP_Text>());

        textMeshPro = list.Find(e => e.name == "InfoEntityTMP");
        infoEntityPrefab = textMeshPro.transform.parent.gameObject;
        
        entityStats = gameObject.GetComponent<EntityStats>();

        entityName = Localization.GetEntityDescription(gameObject.name.Replace("(Clone)", "")).NAME;
    }

    public void OnMouseEnter()
    {
        if (!changeUI.uIIsOpen)
        {
            infoEntityPrefab.SetActive(true);
            infoEntityPrefab.transform.position = cam.WorldToScreenPoint(transform.position);
            textMeshPro.text = entityName + " (" + entityStats.currentHealth + ")";
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
