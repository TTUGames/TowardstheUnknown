using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    private TextMeshProUGUI textField;
    private Vector2 startingPosition;
    private float startTime;

    private static DamageIndicator prefab;

    [SerializeField] private float duration;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

    /// <summary>
    /// Displays damage on UI at the source's position on screen
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="source"></param>
    public static void DisplayDamage(int damage, Transform source) {
        if (prefab == null)
            prefab = Resources.Load<DamageIndicator>("Prefabs/UI/InGameDisplay/DamageIndicator");
        
        DamageIndicator damageIndicator = Instantiate(prefab);
        damageIndicator.textField.text = damage.ToString();
        damageIndicator.transform.SetParent(GameObject.Find("Canvas | MainUI").transform);
        damageIndicator.startingPosition = GameObject.Find("Main Camera").GetComponent<Camera>().WorldToScreenPoint(source.position);
    }

	private void Awake() {
        textField = GetComponent<TextMeshProUGUI>();
        startTime = Time.time;
        if (Random.Range(0f, 1f) > 0.5) xOffset *= -1; //Randomize the sin wave direction
	}


	void Start()
    {
        StartCoroutine(DisappearAfterTime());
    }

	private void Update() {
        RefreshPosition();
    }

    /// <summary>
    /// Destroy itself after the specified duration
    /// </summary>
    /// <returns></returns>
	private IEnumerator DisappearAfterTime() {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
	}

    /// <summary>
    /// Moves the object along a sin wave
    /// </summary>
    private void RefreshPosition() {
        transform.position = new Vector3(startingPosition.x + xOffset * (startTime - Time.time) / duration,
            startingPosition.y - Mathf.Sin((startTime - Time.time) * Mathf.PI / duration) * yOffset, 
            0);
	}
}
