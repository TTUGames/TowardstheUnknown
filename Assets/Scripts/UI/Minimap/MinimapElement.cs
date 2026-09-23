using UnityEngine;
using UnityEngine.UI;

public class MinimapElement : MonoBehaviour
{
	private Image outline;
	private Image icon;


	public static MinimapElement InstantiateElement(Transform parent, RoomType type) {
		GameAssets assets = GameAssets.Instance;
		MinimapElement element = Instantiate(assets.minimapElement, parent);
		Sprite icon = type switch {
			RoomType.TREASURE => assets.minimapTreasure,
			RoomType.BOSS => assets.minimapBoss,
			RoomType.ANTECHAMBER => assets.minimapAntechamber,
			_ => null,
		};
		if (icon != null) element.icon.sprite = icon;
		else Destroy(element.icon);
		return element;
	}

	private void Awake() {
		outline = transform.Find("Outline").GetComponent<Image>();
		icon = transform.Find("Icon").GetComponent<Image>();
	}

	public void SetCurrent(bool current) {
		outline.color = Color.white;
		outline.sprite = current ? GameAssets.Instance.minimapCurrentRoom : GameAssets.Instance.minimapRoom;
	}

	public void SetActive(bool active) {
		if (active ^ !gameObject.activeSelf) return;
		gameObject.SetActive(active);
		outline.color = new Color(0.5f, 0.5f, 0.5f);
	}
}
