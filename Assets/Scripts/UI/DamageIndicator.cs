using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    private TextMeshProUGUI textField;
    private Vector2 position;
    private float startTime;

    [SerializeField] private float xSpeed;
    [SerializeField] private float amplitude;
    [SerializeField] private float frequenz;


    public static void DisplayDamage(int damage, Transform source) {
        DamageIndicator damageIndicator = Instantiate<DamageIndicator>(Resources.Load<DamageIndicator>("Prefabs/UI/TemporaryUI/DamageIndicator"));
        damageIndicator.textField.text = damage.ToString();
        damageIndicator.transform.SetParent(GameObject.Find("Canvas").transform);
        damageIndicator.position = GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(source.position);
        damageIndicator.RefreshPosition();
    }

	private void Awake() {
        textField = GetComponent<TextMeshProUGUI>();
        startTime = Time.time;
	}


	void Start()
    {
        StartCoroutine(DisappearAfterTime());
    }

	private void Update() {
        
    }

	private IEnumerator DisappearAfterTime() {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
	}

    private void RefreshPosition() {
        transform.position = new Vector3(position.x, position.y, 0);
	}
}
