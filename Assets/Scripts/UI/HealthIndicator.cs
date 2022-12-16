using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthIndicator : MonoBehaviour
{
    private TextMeshProUGUI textField;

    private Camera camera;
    public GameObject entityGameObject { get; set; }
    public EntityStats entityStats { get; set; }

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
