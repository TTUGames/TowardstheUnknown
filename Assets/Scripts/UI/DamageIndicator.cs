using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    private TextMeshProUGUI textField;
    private Vector2 startingPosition;
    private static DamageIndicator prefab;
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
	}
    
	void Start()
    {
        transform.position = new Vector3 (startingPosition.x + xOffset, startingPosition.y + yOffset, 0);
    }

    void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
