using UnityEngine;
using UnityEngine.UI;

public class MinimapElement : MonoBehaviour
{
	[SerializeField] private Image outline;
	[SerializeField] private Image icon;

	[Header("Outlines")]
	[SerializeField] private Sprite roomSprite;
	[SerializeField] private Sprite currentRoomSprite;

	[Header("Icons")]
	[SerializeField] private Sprite treasureIcon;
	[SerializeField] private Sprite bossIcon;
	[SerializeField] private Sprite antechamberIcon;


	public void SetType(RoomType type) {
		Sprite sprite = type switch {
			RoomType.TREASURE => treasureIcon,
			RoomType.BOSS => bossIcon,
			RoomType.ANTECHAMBER => antechamberIcon,
			_ => null,
		};
		if (sprite != null) icon.sprite = sprite;
		else Destroy(icon.gameObject);
	}

	public void SetCurrent(bool current) {
		outline.color = Color.white;
		outline.sprite = current ? currentRoomSprite : roomSprite;
	}

	public void SetActive(bool active) {
		if (active ^ !gameObject.activeSelf) return;
		gameObject.SetActive(active);
		outline.color = new Color(0.5f, 0.5f, 0.5f);
	}
}
