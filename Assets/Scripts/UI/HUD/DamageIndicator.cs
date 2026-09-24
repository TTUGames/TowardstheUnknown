using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    private TextMeshProUGUI textField;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

	private void Awake() {
        textField = GetComponent<TextMeshProUGUI>();
	}

    /// <summary>
    /// Displays the damage at the given screen position
    /// </summary>
    public void Show(int damage, Vector2 screenPosition) {
        textField.text = damage.ToString();
        transform.position = new Vector3(screenPosition.x + xOffset, screenPosition.y + yOffset, 0);
    }

    void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
