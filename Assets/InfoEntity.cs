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

    public void Start()
    {
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        
        List<TMP_Text> list = new List<TMP_Text>();
        list.AddRange(Resources.FindObjectsOfTypeAll<TMP_Text>());

        textMeshPro = list.Find(e => e.name == "InfoEntityTMP");
        infoEntityPrefab = textMeshPro.transform.parent.gameObject;
        
        entityStats = gameObject.GetComponent<EnemyStats>();
        if (entityStats == null)
            entityStats = gameObject.GetComponent<PlayerStats>();

            entityName = gameObject.name.Replace("(Clone)", "");
    }

    public void OnMouseOver()
    {
        if (entityStats.currentHealth > 0)
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
