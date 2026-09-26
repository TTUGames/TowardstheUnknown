using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The ring under an entity during the deploy phase and the combat (<c>Rendering/EntityRing.shader</c>): blue for the
/// player, red for the enemies, from its material. It pulses on the entity's turn, brightens while the entity is hovered
/// (<see cref="InfoEntity"/>, the timeline), turns to the target color while the selected artifact would hit it, and fades
/// out when the entity dies. The ring is an object of its own following the entity, so that the entity's renderers
/// (outline, hit flash, dissolve) don't include it.
/// </summary>
[DisallowMultipleComponent]
public class EntityRing : MonoBehaviour
{
    private static readonly int FadeId = Shader.PropertyToID("_Fade");
    private static readonly int ActiveId = Shader.PropertyToID("_Active");
    private static readonly int HoverId = Shader.PropertyToID("_Hover");
    private static readonly int TargetedId = Shader.PropertyToID("_Targeted");
    private static Mesh quad;

    [SerializeField, Tooltip("Rendering/EntityRing.shader: Mat_RingPlayer or Mat_RingEnemy")] private Material material;
    [SerializeField, Tooltip("Width of the ring's quad in meters, a tile being 1")] private float diameter = 1.1f;
    [SerializeField, Tooltip("Height above the entity's feet, over the grid and the selection overlays")] private float heightOffset = 0.015f;
    [SerializeField, Tooltip("Seconds to show or hide the ring")] private float fadeDuration = 0.35f;
    [SerializeField, Tooltip("Seconds to switch the turn, hover and target states")] private float stateDuration = 0.15f;

    private readonly float[] current = new float[4];
    private readonly float[] target = new float[4];
    private const int FADE = 0, ACTIVE = 1, HOVER = 2, TARGETED = 3;

    private Transform ring;
    private MeshRenderer ringRenderer;
    private MaterialPropertyBlock block;
    private EntityStats stats;
    private EntityTurn turn;
    private PlayerAttack playerAttack;
    private bool shown;
    private bool dirty = true;

    /// <summary>
    /// The entity is hovered, on the board or in the timeline
    /// </summary>
    public bool Hovered { set => target[HOVER] = value ? 1f : 0f; }

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
        turn = GetComponent<EntityTurn>();
        block = new MaterialPropertyBlock();

        var ringObject = new GameObject(name + " Ring");
        ring = ringObject.transform;
        ring.localScale = new Vector3(diameter, 1f, diameter);
        ringObject.AddComponent<MeshFilter>().sharedMesh = Quad();
        ringRenderer = ringObject.AddComponent<MeshRenderer>();
        ringRenderer.sharedMaterial = material;
        ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ringRenderer.receiveShadows = false;
        ringRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        ringRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        ringRenderer.enabled = false;
    }

    private void OnEnable()
    {
        GameEvents.DeployStarted += Show;
        GameEvents.CombatStarted += Show;
        GameEvents.CombatEnded += Hide;
        GameEvents.RunEnded += OnRunEnded;
        GameEvents.RoomLeft += Hide;
        if (stats != null) stats.Died += OnDied;
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
        if (GameScene.Player != null && GameScene.Player.TryGetComponent(out playerAttack))
            playerAttack.TargetsPreviewed += OnTargetsPreviewed;

        if (ring != null) ring.gameObject.SetActive(true);
        shown = TurnSystem.Instance.IsCombat;
        OnTurnChanged();
    }

    private void OnDisable()
    {
        GameEvents.DeployStarted -= Show;
        GameEvents.CombatStarted -= Show;
        GameEvents.CombatEnded -= Hide;
        GameEvents.RunEnded -= OnRunEnded;
        GameEvents.RoomLeft -= Hide;
        if (stats != null) stats.Died -= OnDied;
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
        if (playerAttack != null) playerAttack.TargetsPreviewed -= OnTargetsPreviewed;
        playerAttack = null;

        if (ring != null) ring.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ring != null) Destroy(ring.gameObject);
    }

    private void LateUpdate()
    {
        target[FADE] = shown && (stats == null || !stats.IsDead) ? 1f : 0f;
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] == target[i]) continue;
            float duration = i == FADE ? fadeDuration : stateDuration;
            current[i] = Mathf.MoveTowards(current[i], target[i], Time.deltaTime / Mathf.Max(duration, 0.001f));
            dirty = true;
        }

        ringRenderer.enabled = current[FADE] > 0f;
        if (!ringRenderer.enabled) return;
        ring.SetPositionAndRotation(transform.position + Vector3.up * heightOffset, Quaternion.identity);
        if (!dirty) return;
        dirty = false;
        block.SetFloat(FadeId, current[FADE]);
        block.SetFloat(ActiveId, current[ACTIVE]);
        block.SetFloat(HoverId, current[HOVER]);
        block.SetFloat(TargetedId, current[TARGETED]);
        ringRenderer.SetPropertyBlock(block);
    }

    private void Show() => shown = true;

    private void Hide() => shown = false;

    private void OnRunEnded(bool isVictory) => Hide();

    private void OnDied()
    {
        target[ACTIVE] = 0f;
        target[HOVER] = 0f;
        target[TARGETED] = 0f;
    }

    private void OnTurnChanged() =>
        target[ACTIVE] = turn != null && TurnSystem.Instance.IsCurrentTurn(turn) ? 1f : 0f;

    private void OnTargetsPreviewed(Artifact artifact, IReadOnlyList<EntityStats> targets)
    {
        bool targeted = false;
        if (stats != null)
            foreach (EntityStats entity in targets)
                if (entity == stats) targeted = true;
        target[TARGETED] = targeted ? 1f : 0f;
    }

    //A flat quad on the ground, 1 meter wide, shared by all the rings
    private static Mesh Quad()
    {
        if (quad != null) return quad;
        quad = new Mesh { name = "Entity Ring Quad" };
        quad.SetVertices(new[] { new Vector3(-0.5f, 0f, -0.5f), new Vector3(-0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, -0.5f) });
        quad.SetUVs(0, new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) });
        quad.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
        quad.RecalculateBounds();
        return quad;
    }
}
