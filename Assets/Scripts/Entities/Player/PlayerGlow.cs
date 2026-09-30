using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The player's neons: the glowing outfit (Character Glow shader) and the weapons (HDR glow color, times <c>intensity</c>).
/// Casting tints them in the artifact's color and flashes the outfit, then they go back to the rest color, the outfit
/// material's. The outfit's glow also follows the game: full in exploration, following the energy left during the player's
/// turn, dimmed during the enemies' turns (<c>_GlowMultiplier</c>, eased). The outfit is set through property blocks on its
/// shared materials, which the edition swaps (<see cref="EditionProfile.outfitColorProperty"/>: the Classic's outfit shader
/// names its color otherwise, and has no glow level). With <see cref="EditionProfile.outfitColorOnMaterials"/>, the color goes
/// on instances of those materials instead, as the original's ChangeColor did: its glow shader renders a property block's
/// HDR color much brighter. The weapons take theirs through property blocks in both editions, as their material would
/// (<see cref="SetWeaponColor"/>), so that the edition can swap their shared materials
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

    private readonly List<Renderer> outfit = new List<Renderer>();
    private readonly List<Renderer> weapons = new List<Renderer>();
    // The outfit's material instances, with outfitColorOnMaterials
    private readonly Dictionary<Renderer, Material> outfitInstances = new Dictionary<Renderer, Material>();
    // The material each instance was made from, put back when the instance goes and nothing else replaced it (a material
    // the edition doesn't pair, which EditionMaterials doesn't give back)
    private readonly Dictionary<Renderer, Material> outfitSources = new Dictionary<Renderer, Material>();
    private MaterialPropertyBlock block;
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
        Edition.Changed += OnEditionChanged;
        stats.EnergyChanged += RefreshLevel;
        GameEvents.CombatStarted += RefreshLevel;
        GameEvents.CombatEnded += RefreshLevel;
        GameEvents.ExplorationStarted += RefreshLevel;
    }

    private void OnDisable()
    {
        Edition.Changed -= OnEditionChanged;
        stats.EnergyChanged -= RefreshLevel;
        GameEvents.CombatStarted -= RefreshLevel;
        GameEvents.CombatEnded -= RefreshLevel;
        GameEvents.ExplorationStarted -= RefreshLevel;
    }

    private void OnDestroy()
    {
        if (turnSystem != null) turnSystem.TurnChanged -= RefreshLevel;
        ForgetOutfitInstances();
    }

    private void ForgetOutfitInstances()
    {
        foreach (KeyValuePair<Renderer, Material> pair in outfitInstances)
        {
            if (pair.Key != null && pair.Key.sharedMaterial == pair.Value && outfitSources.TryGetValue(pair.Key, out Material source))
                pair.Key.sharedMaterial = source;
            Destroy(pair.Value);
        }
        outfitInstances.Clear();
        outfitSources.Clear();
    }

    // The renderer's material instance, made again when the edition swapped its material
    private Material OutfitInstance(Renderer renderer)
    {
        if (outfitInstances.TryGetValue(renderer, out Material instance) && renderer.sharedMaterial == instance) return instance;
        if (instance != null) Destroy(instance);
        outfitSources[renderer] = renderer.sharedMaterial;
        instance = new Material(renderer.sharedMaterial);
        renderer.sharedMaterial = instance;
        outfitInstances[renderer] = instance;
        return instance;
    }

    public void Start()
    {
        block = new MaterialPropertyBlock();
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
            outfit.Add(neonObject.GetComponent<SkinnedMeshRenderer>());
        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            weapons.Add(neonObject.GetComponent<MeshRenderer>());
        restColor = RestColor();
        currentColor = restColor;
        ApplyColor(restColor);
        // The turn system is another object of the scene: it is ready once every Awake has run
        turnSystem = TurnSystem.Instance;
        turnSystem.TurnChanged += RefreshLevel;
        RefreshLevel();
        level = targetLevel;
    }

    // The outfit material's color, in the current edition's property
    private Color RestColor()
    {
        EditionProfile profile = Edition.Profile;
        if (profile.neonRestColor.a > 0) return profile.neonRestColor;
        Material material = outfit.Count > 0 ? outfit[0].sharedMaterial : null;
        string property = profile.outfitColorProperty;
        return material != null && material.HasColor(property) ? material.GetColor(property) : Color.white;
    }

    /// <summary>
    /// The outfit got the edition's materials: its properties start over from them
    /// </summary>
    private void OnEditionChanged(GameEdition edition)
    {
        if (block == null) return;
        foreach (Renderer renderer in outfit) renderer.SetPropertyBlock(null);
        // The edition gave the outfit its materials back: the instances are made again from them
        ForgetOutfitInstances();
        StopAllCoroutines();
        restColor = RestColor();
        appliedMultiplier = -1;
        ApplyColor(restColor);
        ApplyMultiplier(level + flash);
    }

    /// <summary>
    /// Flashes the outfit and, with the edition's <see cref="EditionProfile.castTint"/> (the Classic), tints the neons in the
    /// cast artifact's color
    /// </summary>
    public void Colorize(Color color)
    {
        flash = castFlash;
        animating = true;
        if (Edition.Profile.castTint) TintTo(color);
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
        if (!Edition.Profile.outfitGlowLevel || Mathf.Approximately(multiplier, appliedMultiplier)) return;
        appliedMultiplier = multiplier;
        foreach (Renderer renderer in outfit)
        {
            renderer.GetPropertyBlock(block);
            block.SetFloat(GlowMultiplier, multiplier);
            renderer.SetPropertyBlock(block);
        }
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
        EditionProfile profile = Edition.Profile;
        Color outfitColor = profile.neonIntensity > 0 ? color * profile.neonIntensity : color;
        Color weaponColor = color * (profile.neonIntensity > 0 ? profile.neonIntensity : intensity);
        foreach (Renderer renderer in outfit)
        {
            if (profile.outfitColorOnMaterials)
            {
                OutfitInstance(renderer).SetColor(profile.outfitColorProperty, outfitColor);
                continue;
            }
            renderer.GetPropertyBlock(block);
            block.SetColor(profile.outfitColorProperty, outfitColor);
            renderer.SetPropertyBlock(block);
        }
        foreach (Renderer weapon in weapons)
        {
            weapon.GetPropertyBlock(block);
            SetWeaponColor(weapon.sharedMaterial, weaponColor);
            weapon.SetPropertyBlock(block);
        }
    }

    /// <summary>
    /// Sets the glow color in the block as the material would take it: a block converts its colors to linear, which an HDR
    /// property (the Classic's weapon shader) takes as they are
    /// </summary>
    private void SetWeaponColor(Material material, Color color)
    {
        Shader shader = material.shader;
        int property = shader.FindPropertyIndex("_GlowColor");
        if (property >= 0 && (shader.GetPropertyFlags(property) & ShaderPropertyFlags.HDR) != 0) block.SetVector(GlowColor, color);
        else block.SetColor(GlowColor, color);
    }
}
