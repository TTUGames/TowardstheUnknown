using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    private TextMeshProUGUI textField;
    private Vector2 startingPosition;
    private float startTime;

    [SerializeField] private float duration;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;


    public static void DisplayDamage(int damage, Transform source) {
        DamageIndicator damageIndicator = Instantiate<DamageIndicator>(Resources.Load<DamageIndicator>("Prefabs/UI/TemporaryUI/DamageIndicator"));
        damageIndicator.textField.text = damage.ToString();
        damageIndicator.transform.SetParent(GameObject.Find("Canvas").transform);
        damageIndicator.startingPosition = GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(source.position);
    }

	private void Awake() {
        textField = GetComponent<TextMeshProUGUI>();
        startTime = Time.time;
        if (Random.Range(0f, 1f) > 0.5) xOffset *= -1;
	}


	void Start()
    {
        StartCoroutine(DisappearAfterTime());
    }

	private void Update() {
        RefreshPosition();
    }

	private IEnumerator DisappearAfterTime() {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
	}

    private void RefreshPosition() {
        transform.position = new Vector3(startingPosition.x + xOffset * (startTime - Time.time) / duration,
            startingPosition.y - Mathf.Sin((startTime - Time.time) * Mathf.PI / duration) * yOffset, 
            0);
	}
}
