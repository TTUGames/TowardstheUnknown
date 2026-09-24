using UnityEngine;

/// <summary>
/// An exit of a room, leading to the adjacent room in its direction
/// </summary>
public class TransitionTile : MonoBehaviour
{
    public Direction direction = Direction.NORTH;

    private GameObject vfx;

    /// <summary>
    /// Adds the VFX shown while the exit is open. The exit starts closed.
    /// </summary>
    public void AddVFX(GameObject vfxPrefab) {
        vfx = Instantiate(vfxPrefab);
        vfx.transform.SetParent(transform);
        vfx.transform.position = transform.position + Vector3.up * 0.55f;
        vfx.SetActive(false);
    }

    public void SetOpen(bool open) {
        if (vfx != null) vfx.SetActive(open);
    }
}
