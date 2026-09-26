using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The player's neons: the glowing outfit (Character Glow shader) and the weapons (HDR glow color, times <c>intensity</c>).
/// Casting tints them in the artifact's color and flashes the outfit, then they go back to the rest color, the outfit
/// material's. The outfit's glow also follows the game: full in exploration, following the energy left during the player's
/// turn, dimmed during the enemies' turns (<c>_GlowMultiplier</c>, eased)
/// </summary>
public class PlayerGlow : MonoBehaviour
{
    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
    private static readonly int GlowMultiplier = Shader.PropertyToID("_GlowMultiplier");

    [SerializeField] List<GameObject> lNeonObjectWithSkinnedMeshRenderer;
    [SerializeField] List<GameObject> lNeonObjectWithMeshRenderer;
    [Tooltip("HDR multiplier of the weapons' glow color")]
    [SerializeField] float intensity;

    [Header("Glow level")]
    [Tooltip("Outfit glow with no energy left, during the player's turn (1 at full energy)")]
    [SerializeField, Range(0, 1)] float emptyEnergyGlow = 0.35f;
    [Tooltip("Outfit glow during the enemies' turns")]
    [SerializeField, Range(0, 1)] float enemyTurnGlow = 0.5f;
    [Tooltip("Outfit glow added when casting, fading out over flashDuration")]
    [SerializeField] float castFlash = 1.5f;
    [SerializeField] float flashDuration = 0.5f;
    [Tooltip("How fast the glow eases towards its level, per second")]
    [SerializeField] float easing = 4f;

    private readonly List<Material> outfitMaterials = new List<Material>();
    private readonly List<Material> weaponMaterials = new List<Material>();
    private Color restColor;
    private Color currentColor;
    private PlayerStats stats;
    private PlayerTurn turn;
    private TurnSystem turnSystem;
    // Whether the level or the flash still moves: Update sleeps otherwise
    private bool animating;
    private float level = 1;
    private float targetLevel = 1;
    private float flash;
    private float appliedMultiplier = -1;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        turn = GetComponent<PlayerTurn>();
    }

    private void OnEnable()
    {
        stats.EnergyChanged += RefreshLevel;
        GameEvents.CombatStarted += RefreshLevel;
        GameEvents.CombatEnded += RefreshLevel;
        GameEvents.ExplorationStarted += RefreshLevel;
    }

    private void OnDisable()
    {
        stats.EnergyChanged -= RefreshLevel;
        GameEvents.CombatStarted -= RefreshLevel;
        GameEvents.CombatEnded -= RefreshLevel;
        GameEvents.ExplorationStarted -= RefreshLevel;
    }

    private void OnDestroy()
    {
        if (turnSystem != null) turnSystem.TurnChanged -= RefreshLevel;
    }

    public void Start()
    {
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
            outfitMaterials.Add(neonObject.GetComponent<SkinnedMeshRenderer>().material);
        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            weaponMaterials.Add(neonObject.GetComponent<MeshRenderer>().material);
        restColor = outfitMaterials.Count > 0 ? outfitMaterials[0].GetColor(GlowColor) : Color.white;
        currentColor = restColor;
        ApplyColor(restColor);
        // The turn system is another object of the scene: it is ready once every Awake has run
        turnSystem = TurnSystem.Instance;
        turnSystem.TurnChanged += RefreshLevel;
        RefreshLevel();
        level = targetLevel;
    }

    /// <summary>
    /// Tints the neons in the cast artifact's color and flashes the outfit
    /// </summary>
    public void Colorize(Color color)
    {
        flash = castFlash;
        animating = true;
        TintTo(color);
    }

    public void Uncolorize()
    {
        TintTo(restColor);
    }

    /// <summary>
    /// The glow level the outfit eases towards, for the current phase of the game
    /// </summary>
    private void RefreshLevel()
    {
        if (turnSystem == null || !turnSystem.IsCombat)
            targetLevel = 1;
        else if (turnSystem.IsCurrentTurn(turn))
            targetLevel = Mathf.Lerp(emptyEnergyGlow, 1, stats.MaxEnergy > 0 ? (float)stats.CurrentEnergy / stats.MaxEnergy : 1);
        else
            targetLevel = enemyTurnGlow;
        animating = true;
    }

    /// <summary>
    /// Eases the level and fades the flash out, then sleeps until the level or a flash changes
    /// </summary>
    private void Update()
    {
        if (!animating) return;
        float deltaTime = Time.deltaTime;
        level = Mathf.Lerp(level, targetLevel, 1 - Mathf.Exp(-easing * deltaTime));
        flash = Mathf.MoveTowards(flash, 0, castFlash / Mathf.Max(flashDuration, 0.01f) * deltaTime);
        ApplyMultiplier(level + flash);
        if (flash <= 0 && Mathf.Abs(level - targetLevel) < 0.001f)
        {
            level = targetLevel;
            ApplyMultiplier(level);
            animating = false;
        }
    }

    private void ApplyMultiplier(float multiplier)
    {
        if (Mathf.Approximately(multiplier, appliedMultiplier)) return;
        appliedMultiplier = multiplier;
        foreach (Material material in outfitMaterials)
            material.SetFloat(GlowMultiplier, multiplier);
    }

    private void TintTo(Color targetColor)
    {
        StopAllCoroutines();
        StartCoroutine(ColorTransition(targetColor));
    }

    private IEnumerator ColorTransition(Color targetColor)
    {
        Color startingColor = currentColor;
        for (float elapsedTime = 0; elapsedTime < 1f; )
        {
            elapsedTime += Time.deltaTime;
            ApplyColor(Color.Lerp(startingColor, targetColor, elapsedTime));
            yield return null;
        }
    }

    private void ApplyColor(Color color)
    {
        currentColor = color;
        foreach (Material material in outfitMaterials)
            material.SetColor(GlowColor, color);
        foreach (Material material in weaponMaterials)
            material.SetColor(GlowColor, color * intensity);
    }
}
