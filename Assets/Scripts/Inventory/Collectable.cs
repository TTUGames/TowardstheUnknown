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

    private List<Artifact> artifacts;
    private ArtifactRarity bestRarity;
    private Tile tile;
    private GameObject aura;
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

    private void OnEnable() => Edition.Changed += OnEditionChanged;

    private void OnDisable() => Edition.Changed -= OnEditionChanged;

    private void OnEditionChanged(GameEdition edition) => ShowAura();

    /// <summary>
    /// The aura of the best rarity, the current edition's one
    /// </summary>
    private void ShowAura() {
        if (artifacts == null || artifacts.Count == 0) return;
        if (aura != null) Destroy(aura);
        bestRarity = artifacts.Max(artifact => artifact.Rarity);
        aura = Instantiate(GameAssets.Instance.classicSkin.Resolve(auras[(int)bestRarity]), transform);
        aura.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Registers on the tile under it, so that the movement paths go around it. Done in Start, once the spawn point has placed it
    /// </summary>
    private void Start() {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))
            && hit.collider.TryGetComponent(out tile))
            tile.Collectable = this;
        room = GetComponentInParent<Room>();
        if (room != null) room.CountLoot(1);
    }

    private void OnDestroy() {
        if (tile != null && tile.Collectable == this) tile.Collectable = null;
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
    /// Opens the chest interface with this collectable's artifacts, and destroys it. With the edition's
    /// <see cref="EditionProfile.chestReveal"/>, the relic bursts first and the chest opens a moment later, the queue holding the player
    /// </summary>
    private void TryPickUp()
    {
        if (artifacts == null) throw new System.Exception("Collectable should not be instantiated directly, SetArtifacts must be called after instantiating it");
        if (!Edition.Profile.chestReveal || openBurst == null || palette == null)
        {
            GameScene.UI.Inventory.OpenChest(artifacts);
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
        ActionManager.AddToBottom(() => GameScene.UI.Inventory.OpenChest(content));
        Destroy(gameObject);
    }
}
