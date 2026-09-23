using UnityEngine;

public class TransitionTile : MonoBehaviour
{
    public Direction direction = Direction.NORTH;
	private static GameObject vfxPrefab;
    [HideInInspector] public GameObject vfx;

	private void Awake() {
		if (vfxPrefab == null) vfxPrefab = Resources.Load<GameObject>("VFX/SwitchMap");
		vfx = Instantiate(vfxPrefab);
		vfx.SetActive(false);
		vfx.transform.SetParent(transform);
		vfx.transform.position = transform.position + Vector3.up * 0.55f;
	}
}
