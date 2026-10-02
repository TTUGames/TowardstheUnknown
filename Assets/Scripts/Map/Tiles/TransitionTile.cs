using UnityEngine;

/// <summary>
/// An exit of a room, leading to the adjacent room in its direction
/// </summary>
public class TransitionTile : MonoBehaviour
{
    public Direction direction = Direction.NORTH;

    // The VFX given, and the edition's one shown (EditionSkin.Resolve)
    private GameObject vfxPrefab;
    private GameObject shownPrefab;
    private GameObject vfx;
    private ExitPortal portal;
    private ExitDestination destination = ExitDestination.COMBAT;
    private bool open;

    /// <summary>
    /// The exit can be taken: its room is not in combat
    /// </summary>
    public bool IsOpen => open;

    private void OnEnable() {
        Edition.Changed += OnEditionChanged;
        // The edition may have changed while the room was left
        if (vfxPrefab != null && Resolve(vfxPrefab) != shownPrefab) AddVFX(vfxPrefab);
    }

    private void OnDisable() => Edition.Changed -= OnEditionChanged;

    private void OnEditionChanged(GameEdition edition) {
        if (vfxPrefab != null) AddVFX(vfxPrefab);
    }

    private static GameObject Resolve(GameObject prefab) => GameAssets.Instance.classicSkin.Resolve(prefab);

    /// <summary>
    /// Adds the edition's VFX shown while the exit is open, replacing the one it had. The exit starts closed.
    /// </summary>
    public void AddVFX(GameObject vfxPrefab) {
        if (vfx != null) Destroy(vfx);
        this.vfxPrefab = vfxPrefab;
        shownPrefab = Resolve(vfxPrefab);
        vfx = Instantiate(shownPrefab);
        vfx.transform.SetParent(transform);
        vfx.transform.position = transform.position + Vector3.up * 0.55f;
        portal = vfx.GetComponent<ExitPortal>();
        vfx.SetActive(false);
        SetDestination(destination);
        if (this.open) SetOpen(true);
    }

    /// <summary>
    /// Highlights the portal while the pointer is on the exit
    /// </summary>
    public void SetHovered(bool hovered) {
        if (portal != null) portal.SetHovered(hovered);
    }

    /// <summary>
    /// Tells the VFX where the exit leads, when it shows it (the Anniversary's <see cref="ExitPortal"/>)
    /// </summary>
    public void SetDestination(ExitDestination destination) {
        this.destination = destination;
        if (portal != null) portal.SetDestination(destination);
    }

    /// <summary>
    /// Shows or hides the VFX, through its reveal and closing when it is an <see cref="ExitPortal"/>
    /// </summary>
    public void SetOpen(bool open) {
        this.open = open;
        if (portal != null) portal.SetOpen(open);
        else if (vfx != null) vfx.SetActive(open);
    }
}
