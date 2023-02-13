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
	}

	private void Awake() {
		outline = transform.Find("Outline").GetComponent<Image>();
		icon = transform.Find("Icon").GetComponent<Image>();
	}

	public void SetCurrent(bool current) {
		if (current) outline.color = new Color(68f/255f, 214f/255f, 44f/255f);
		else outline.color = Color.white;
	}

	public void SetActive(bool active) {
		gameObject.SetActive(active);
	}
}
