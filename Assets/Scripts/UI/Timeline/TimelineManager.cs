using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class TimelineManager : MonoBehaviour
{

    public GameObject timelineItemPrefab;

    [Space]
    public float itemSize = 0.025f;
    public float spacing = 0.25f;

    private RectTransform timelineRT;
    private GameObject[] timelineItems = new GameObject[0];

    private void Awake()
    {
        timelineRT = GetComponent<RectTransform>();
    }

    void Start()
    {
        // InvokeRepeating("DisplayEntity", 0f, 5f);
    }
    
    void Update()
    {
        // timelineRT.anchorMin = new Vector2(0.5f - itemSize * timelineItems.Length * spacing, timelineRT.anchorMin.y);
        // timelineRT.anchorMax = new Vector2(0.5f + itemSize * timelineItems.Length * spacing, timelineRT.anchorMax.y);
    }

    public void UpdateTimeline()
    {
        foreach (GameObject timelineItem in timelineItems)
            Destroy(timelineItem);

        List<GameObject> entities = FindObjectsOfType<GameObject>().Where(obj => obj.GetComponent<PlayerStats>() != null || obj.GetComponent<EnemyStats>() != null).ToList();
        entities.Reverse();
        CreateItems(entities);

        // timelineRT.anchorMin = new Vector2(0.5f - itemSize * timelineItems.Length * spacing, timelineRT.anchorMin.y);
        // timelineRT.anchorMax = new Vector2(0.5f + itemSize * timelineItems.Length * spacing, timelineRT.anchorMax.y);

        // float previousAnchorMaxPoint = 0f;
        // float anchorMaxXSize = (1f - previousAnchorMaxPoint) / timelineItems.Length;

        for (int i = 0; i < timelineItems.Length; i++)
        {
            GameObject item = timelineItems[i];

            RectTransform itemRT = item.GetComponent<RectTransform>();
            // itemRT.localScale = new Vector3(1, 1, 1);
            // itemRT.anchoredPosition = new Vector2(0.5f, 0.5f);
            itemRT.anchorMin = new Vector2(i * spacing, 0);
            itemRT.anchorMax = new Vector2((i + 1) * spacing, 1f);
            // itemRT.anchorMin = new Vector2(previousAnchorMaxPoint, 0);
            // itemRT.anchorMax = new Vector2(previousAnchorMaxPoint + anchorMaxXSize, 1f);
            // previousAnchorMaxPoint = itemRT.anchorMax.x;
            // itemRT.offsetMin = new Vector2(0, 0);
            // itemRT.offsetMax = new Vector2(0, 0);

            DisplayStats displayStats = item.GetComponent<DisplayStats>();
            displayStats.SetEntityStats(entities[i]);
        }
    }

    private void CreateItems(List<GameObject> entities)
    {
        timelineItems = new GameObject[entities.Count()];

        for (int i = 0; i < timelineItems.Length; i++)
        {
            GameObject timelineItem = Instantiate(timelineItemPrefab, transform);
            timelineItem.name = "TimelineItem" + i;
            timelineItem.layer = gameObject.layer;
            timelineItems[i] = timelineItem;
        }
    }

    void DisplayEntity()
    {
        var objects = FindObjectsOfType<GameObject>();
        var filteredObjects = objects.Where(obj => obj.GetComponent<PlayerStats>() != null || obj.GetComponent<EnemyStats>() != null);

        string list = "Nombre d'entités : " + filteredObjects.Count();
        
        
        
        // Affiche la liste dans la console
        Debug.Log(list);
    }
}
