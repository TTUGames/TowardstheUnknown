using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapElement : MonoBehaviour
{
	private static MinimapElement prefab;
	private static Sprite treasureRoomTexture;
	private static Sprite bossRoomTexture;
	private static Sprite antechamberRoomTexture;

	private static Sprite standardOutline;
	private static Sprite currentRoomOutline;

	private Image outline;
	private Image icon;


	public static MinimapElement InstantiateElement(Transform parent, RoomType type) {
		if (prefab == null) InitStaticValues();
		MinimapElement element = Instantiate<MinimapElement>(prefab, parent);

		switch (type) {
			case RoomType.TREASURE:
				element.icon.sprite = treasureRoomTexture;
				break;
			case RoomType.BOSS:
				element.icon.sprite = bossRoomTexture;
				break;
			case RoomType.ANTECHAMBER:
				element.icon.sprite = antechamberRoomTexture;
				break;
			default:
				Destroy(element.icon);
				break;
		}

		return element;
	}

	private static void InitStaticValues() {
		prefab = Resources.Load<MinimapElement>("Prefabs/UI/Minimap/MinimapElement");
		treasureRoomTexture = Resources.Load<Sprite>("UI/Minimap/TreasureIcon");
		bossRoomTexture = Resources.Load<Sprite>("UI/Minimap/BossIcon");
		antechamberRoomTexture = Resources.Load<Sprite>("UI/Minimap/AntechamberIcon");

		standardOutline = Resources.Load<Sprite>("UI/Minimap/Room");
		currentRoomOutline = Resources.Load<Sprite>("UI/Minimap/ActiveRoom");
	}

	private void Awake() {
		outline = transform.Find("Outline").GetComponent<Image>();
		icon = transform.Find("Icon").GetComponent<Image>();
	}

	public void SetCurrent(bool current) {
		outline.color = Color.white;
		if (current) outline.sprite = currentRoomOutline;
		else outline.sprite = standardOutline;
	}

	public void SetActive(bool active) {
		if (active ^ !gameObject.activeSelf) return;
		gameObject.SetActive(active);
		outline.color = new Color(0.5f, 0.5f, 0.5f);
	}
}
