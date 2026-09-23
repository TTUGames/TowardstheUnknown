using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineManager : MonoBehaviour
{
    public GameObject timelineItemPrefab;

    [Space]
    public float spacing = 0.25f;

    private GameObject[] timelineItems = new GameObject[0];

    public void UpdateTimeline()
    {
        foreach (GameObject timelineItem in timelineItems)
            Destroy(timelineItem);

        IReadOnlyList<EntityTurn> entities = TurnSystem.Instance.Turns;

        timelineItems = new GameObject[entities.Count];
        for (int i = 0; i < entities.Count; i++)
        {
            GameObject entity = entities[i].gameObject;
            string entityName = entity.name.Replace("(Clone)", "");

            GameObject item = Instantiate(timelineItemPrefab, transform);
            item.name = "TimelineItem" + i;
            item.layer = gameObject.layer;
            timelineItems[i] = item;

            RectTransform itemRT = item.GetComponent<RectTransform>();
            itemRT.anchorMin = new Vector2(i * spacing, 0);
            itemRT.anchorMax = new Vector2((i + 1) * spacing, 1f);

            DisplayStats displayStats = item.GetComponent<DisplayStats>();
            displayStats.SetEntityStats(entities[i].stats);
            displayStats.entity = entity;
            displayStats.entityName = Localization.GetEntityDescription(entityName).NAME;

            item.GetComponentInChildren<Image>().sprite = entities[i].stats.TimelineIcon;
        }
    }
}
