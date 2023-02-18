using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthIndicator : MonoBehaviour
{
    private TextMeshProUGUI textField;

    private Camera mainCamera;
    public GameObject entityGameObject { get; set; }
    public EntityStats entityStats { get; set; }

    public float xOffset;
    public float yOffset;

	private void Awake() {
        textField = GetComponent<TextMeshProUGUI>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        transform.SetAsFirstSibling();
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

        Vector2 screenPoint = mainCamera.WorldToScreenPoint(entityGameObject.transform.position);
        transform.position = new Vector3(screenPoint.x + xOffset, screenPoint.y + yOffset, 0);

        if (entityStats.CurrentHealth > 0)
            textField.text = entityStats.CurrentHealth.ToString();
        else
            textField.text = "";
    }

}
