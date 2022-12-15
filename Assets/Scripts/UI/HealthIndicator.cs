using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthIndicator : MonoBehaviour
{
    private static HealthIndicator prefab;
    /// <summary>
    /// Displays health on UI at the source position on screen
    /// </summary>
    /// <param name="source"></param>
    public static HealthIndicator DisplayHealth(GameObject entityGameObject, EntityStats entityStats) {
        if (prefab == null)
            prefab = Resources.Load<HealthIndicator>("Prefabs/UI/InGameDisplay/HealthIndicator");
        
        HealthIndicator healthIndicator = Instantiate(prefab);
        healthIndicator.entityGameObject = entityGameObject;
        healthIndicator.entityStats = entityStats;
        healthIndicator.transform.SetParent(GameObject.Find("Canvas").transform);

        return healthIndicator;
    }

    private TextMeshProUGUI textField;

    private Camera camera;
    private GameObject entityGameObject;
    private EntityStats entityStats;

    public float xOffset;
    public float yOffset;

	private void Awake() {
        textField = GetComponent<TextMeshProUGUI>();
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
	}


	void Start()
    {
    }

	private void Update() {
        if (entityGameObject == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 screenPoint = camera.WorldToScreenPoint(entityGameObject.transform.position);
        transform.position = new Vector3(screenPoint.x + xOffset, screenPoint.y + yOffset, 0);

        if (entityStats.CurrentHealth > 0)
            textField.text = entityStats.CurrentHealth.ToString();
        else
            textField.text = "";
    }

}
