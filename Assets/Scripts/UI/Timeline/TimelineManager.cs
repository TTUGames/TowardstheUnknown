using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

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

    public void UpdateTimeline()
    {
        foreach (GameObject timelineItem in timelineItems)
            Destroy(timelineItem);

        List<GameObject> entities = FindObjectsOfType<GameObject>().Where(obj => obj.GetComponent<PlayerStats>() != null || obj.GetComponent<EnemyStats>() != null).ToList();
        entities.Reverse();
        CreateItems(entities);

        for (int i = 0; i < timelineItems.Length; i++)
        {
            GameObject item = timelineItems[i];

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(i * spacing, 0);
            itemRT.anchorMax = new Vector2((i + 1) * spacing, 1f);

            DisplayStats displayStats = item.GetComponent<DisplayStats>();
            displayStats.SetEntityStats(entities[i]);

            Image image = item.GetComponentInChildren<Image>();
            image.sprite = (Sprite) Resources.Load("UI/Timeline/" + entities[i].name.Replace("(Clone)", "") + "Icon", typeof(Sprite));
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
}
