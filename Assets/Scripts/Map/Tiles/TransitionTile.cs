using UnityEngine;

public class TransitionTile : MonoBehaviour
{
    public Direction direction = Direction.NORTH;
    [HideInInspector] public GameObject vfx;

	private void Awake() {
		vfx = Instantiate(GameAssets.Instance.roomExit);
		vfx.SetActive(false);
		vfx.transform.SetParent(transform);
		vfx.transform.position = transform.position + Vector3.up * 0.55f;
	}
}
