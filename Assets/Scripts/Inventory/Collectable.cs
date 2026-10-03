using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField, Tooltip("Indexed by the best artifact rarity: common, rare, epic, legendary")]
    private GameObject[] auras = new GameObject[4];
    [SerializeField, Min(0.01f), Tooltip("Seconds a combat's reward takes to grow out of its tile (PopIn)")]
    private float popInDuration = 0.5f;
    [SerializeField, Tooltip("The rarities' colors, of the opening's burst")]
    private RarityPalette palette;
    [SerializeField, Tooltip("Bursts from the relic as it opens, in its best rarity's color (EditionProfile.chestReveal)")]
    private ParticleSystem openBurst;
    [SerializeField, Min(0), Tooltip("Seconds between the burst and the chest opening, the player waiting")]
    private float openDelay = 0.1f;
    [SerializeField, Tooltip("Indexed by the best artifact rarity: the relic bursting, posted on the player as it is destroyed at once")]
    private AK.Wwise.Event[] openSounds = new AK.Wwise.Event[4];
    [SerializeField, Min(0), Tooltip("Meters around its center within which the pointer is on the relic, picking its tile (EditionProfile.modelPicking)")]
    private float pickRadius = 0.35f;

    // The relics lying on a tile, which the pointer can pick
    private static readonly List<Collectable> lying = new();

    private List<Artifact> artifacts;
    private ArtifactRarity bestRarity;
    private Tile tile;
    private GameObject aura;
    // The aura's answer to the pointer: the Anniversary's relic only
    private RelicHover hover;
    private bool hovered;
    // The room it lies in, which counts it as its loot
    private Room room;

    public void SetArtifacts(List<Artifact> artifacts) {
        this.artifacts = artifacts;
        ShowAura();
        //Before the player can pick them up and cast them
        VFXWarmup.Warm(artifacts);
	}

    /// <summary>
    /// Grows out of its tile, overshooting a little before it settles: a combat's reward, after the victory's beat
    /// </summary>
    public void PopIn() => StartCoroutine(PopInRoutine());

    private IEnumerator PopInRoutine() {
        Vector3 size = transform.localScale;
        for (float time = 0; time < popInDuration; time += Time.deltaTime) {
            transform.localScale = size * EaseOutBack(time / popInDuration);
            yield return null;
        }
        transform.localScale = size;
    }

    private static float EaseOutBack(float t) {
        const float overshoot = 1.70158f;
        t -= 1;
        return 1 + t * t * ((overshoot + 1) * t + overshoot);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => lying.Clear();

    private void OnEnable() {
        Edition.Changed += OnEditionChanged;
        BoardPointer.TileHovered += OnTileHovered;
        // Back with its room, the pointer maybe already on it
        if (tile != null) OnTileHovered(BoardPointer.HoveredTile);
    }

    private void OnDisable() {
        Edition.Changed -= OnEditionChanged;
        BoardPointer.TileHovered -= OnTileHovered;
        SetHovered(false);
    }

    private void OnEditionChanged(GameEdition edition) => ShowAura();

    private void OnTileHovered(Tile hoveredTile) => SetHovered(tile != null && hoveredTile == tile);

    private void SetHovered(bool hovered) {
        this.hovered = hovered;
        if (hover != null) hover.SetHovered(hovered);
    }

    /// <summary>
    /// The tile of the first relic the ray goes through within its <see cref="pickRadius"/>, and the distance at which it does
    /// </summary>
    public static Tile FindPointed(Ray ray, out float distance) {
        distance = Mathf.Infinity;
        Tile pointed = null;
        foreach (Collectable collectable in lying) {
            if (!collectable.isActiveAndEnabled) continue;
            Vector3 toCenter = collectable.transform.position - ray.origin;
            float along = Vector3.Dot(toCenter, ray.direction);
            float missSquared = toCenter.sqrMagnitude - along * along;
            float radiusSquared = collectable.pickRadius * collectable.pickRadius;
            if (along < 0 || missSquared > radiusSquared) continue;
            float hit = along - Mathf.Sqrt(radiusSquared - missSquared);
            if (hit >= distance) continue;
            distance = hit;
            pointed = collectable.tile;
        }
        return pointed;
    }

    /// <summary>
    /// The aura of the best rarity, the current edition's one
    /// </summary>
    private void ShowAura() {
        if (artifacts == null || artifacts.Count == 0) return;
        if (aura != null) Destroy(aura);
        bestRarity = artifacts.Max(artifact => artifact.Data.rarity);
        aura = Instantiate(GameAssets.Instance.classicSkin.Resolve(auras[(int)bestRarity]), transform);
        aura.transform.localPosition = Vector3.zero;
        hover = aura.GetComponent<RelicHover>();
        if (hover != null && hovered) hover.SetHovered(true);
    }

    /// <summary>
    /// Registers on the tile under it, so that the movement paths go around it. Done in Start, once the spawn point has placed it
    /// </summary>
    private void Start() {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))
            && hit.collider.TryGetComponent(out tile)) {
            tile.Collectable = this;
            lying.Add(this);
            OnTileHovered(BoardPointer.HoveredTile);
        }
        room = GetComponentInParent<Room>();
        if (room != null) room.CountLoot(1);
    }

    private void OnDestroy() {
        if (tile != null && tile.Collectable == this) tile.Collectable = null;
        lying.Remove(this);
        // Picked up, not unloaded with the scene
        if (room != null && gameObject.scene.isLoaded) room.CountLoot(-1);
    }

    /// <summary>
    /// Picks the artifacts up when the player walks into it
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerMove player)) return;
        player.InterruptMovement();
        TryPickUp();
    }

    /// <summary>
    /// Opens this collectable's artifacts in a chest (<see cref="GameEvents.ChestOpened"/>), and destroys it. With the edition's
    /// <see cref="EditionProfile.chestReveal"/>, the relic bursts first and the chest opens a moment later, the queue holding the player
    /// </summary>
    private void TryPickUp()
    {
        if (artifacts == null) throw new System.Exception("Collectable should not be instantiated directly, SetArtifacts must be called after instantiating it");
        if (!Edition.Profile.chestReveal || openBurst == null || palette == null)
        {
            GameEvents.OpenChest(artifacts);
            Destroy(gameObject);
            return;
        }
        // The particles' colors are clamped to 1: their material holds the brightness
        Color color = palette.Get(bestRarity, RarityPalette.Tone.Glow);
        color /= Mathf.Max(color.r, color.g, color.b, 0.0001f);
        color.a = 1;
        EntityParticles.Play(openBurst, aura != null ? aura : gameObject, color);
        AK.Wwise.Event sound = openSounds[(int)bestRarity];
        if (sound != null && sound.IsValid()) sound.Post(GameScene.Player.gameObject);
        List<Artifact> content = artifacts;
        ActionManager.AddToBottom(new WaitAction(openDelay));
        ActionManager.AddToBottom(() => GameEvents.OpenChest(content));
        Destroy(gameObject);
    }
}
