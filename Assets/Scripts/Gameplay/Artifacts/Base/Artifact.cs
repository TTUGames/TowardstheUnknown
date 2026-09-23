using System.Collections.Generic;
using UnityEngine;

public abstract class Artifact
{
    protected static readonly Color Purple = new Color(0.5f, 0f, 0.5f);

    protected string animStateName;
    protected float attackDuration = 0;

    protected List<VFXInfo> vfxInfos = new List<VFXInfo>();

    protected int cost = 0;

    protected string title;
    protected string description;
    protected string effectDescription;
    protected string rangeDescription;
    protected string cooldownDescription;

    protected Color playerColor;
    protected WeaponEnum weapon;

    protected Sprite skillBarIcon;
    protected Sprite inventoryIcon;
    protected ArtifactRarity rarity;

    protected TileSearch range;
    protected int minRange;
    protected int maxRange;
    protected int minArea;
    protected int maxArea;

    protected int maximumUsePerTurn = 0;
    protected int cooldown = 0;

    protected int remainingUsesThisTurn;
    protected int remainingCooldown;
    public List<Vector2Int> slots { get; protected set; } = new List<Vector2Int>();

    protected List<string> targets = new List<string>();

    public Artifact()
    {
        SetValuesFromID();
        InitValues();
        UpdateStringsFromValues();
        TurnStart(); //Inits values to avoid greying the artifact in the skillbar
    }

    /// <summary>
    /// Initializes the artifact's specific values such as range, actions, ...
    /// </summary>
    protected abstract void InitValues();

    /// <summary>
    /// Initializes the artifact's values depending on its ID (VFX, animation, icons)
    /// </summary>
    private void SetValuesFromID()
    {
        string id = GetType().Name;
        ArtifactDescription localized = Localization.GetArtifactDescription(id);
        title = localized.TITLE;
        description = localized.DESCRIPTION;
        effectDescription = localized.EFFECTS;
        rangeDescription = localized.RANGE;
        cooldownDescription = localized.COOLDOWN;

        animStateName = id;
        skillBarIcon = Resources.Load<Sprite>("Sprites/Artifact_SkillsBar/" + id);
        inventoryIcon = Resources.Load<Sprite>("Sprites/Artifact_TetrisInventory/" + id);
    }

    private void UpdateStringsFromValues() {
        rangeDescription = string.Format(rangeDescription, minRange, maxRange, minArea, maxArea);
        cooldownDescription = string.Format(cooldownDescription, cooldown == 0 ? maximumUsePerTurn : cooldown - 1);
    }

    /// <summary>
    /// Sets the tile search used to select the targeted tile, and its range
    /// </summary>
    protected void SetRange(TileSearch tileSearch, int min, int max) {
        range = tileSearch;
        minRange = min;
        maxRange = max;
        range.SetRange(min, max);
    }

    /// <summary>
    /// Adds the VFX named after this artifact
    /// </summary>
    protected void AddVFX(VFXInfo.Target target, float delay = 0f) {
        vfxInfos.Add(new VFXInfo("VFX/" + GetType().Name, target, delay));
    }

    /// <summary>
    /// Builds the artifact's inventory shape from (x, y) cells
    /// </summary>
    protected static List<Vector2Int> Shape(params (int x, int y)[] cells) {
        List<Vector2Int> shape = new List<Vector2Int>(cells.Length);
        foreach ((int x, int y) in cells) shape.Add(new Vector2Int(x, y));
        return shape;
    }

    /// <summary>
    /// Applies energy cost and cast restrictions such as cooldown and max uses per turn
    /// </summary>
    /// <param name="source">The player entity that cast the artifact</param>
    protected void ApplyCosts(PlayerStats source)
    {
        --remainingUsesThisTurn;
        if (remainingUsesThisTurn == 0 && remainingCooldown == 0)
            remainingCooldown = cooldown;
        source.UseEnergy(cost); //Last, as it refreshes the skills bar

    }

    /// <summary>
    /// Tells if the artifact can be cast by the source entity
    /// </summary>
    public bool CanUse(PlayerStats source)
    {
        return source.CurrentEnergy >= cost && remainingCooldown == 0 && (maximumUsePerTurn == 0 || remainingUsesThisTurn > 0);
    }

    /// <summary>
    /// Applies start of combat effects to the artifact
    /// </summary>
    public void ResetConstraints()
    {
        remainingCooldown = 0;
        remainingUsesThisTurn = maximumUsePerTurn;
    }

    /// <summary>
    /// Applies start of turn effects to the artifact
    /// </summary>
    public void TurnStart()
    {
        if (remainingCooldown > 0)
            --remainingCooldown;
        remainingUsesThisTurn = maximumUsePerTurn;
    }

    /// <summary>
    /// Tells if a tile is valid to be targeted
    /// </summary>
    public abstract bool CanTarget(Tile tile);

    /// <summary>
    /// Manages the artifact's targets then applies its effects
    /// </summary>
    /// <param name="source">The entity using the artifact</param>
    /// <param name="tile">The targeted tile</param>
    public abstract void Launch(PlayerAttack source, Tile tile);

    /// <summary>
    /// Gets the tiles targetted by the artifact
    /// </summary>
    public abstract List<Tile> GetTargets(Tile targetedTile);

    /// <summary>
    /// Applies the artifacts' effects
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    protected abstract void ApplyEffects(PlayerStats source, EntityStats target);

    /// <summary>
    /// Plays the artifacts animation and vfx
    /// </summary>
    /// <param name="sourceTile"></param>
    /// <param name="targetTile"></param>
    /// <param name="source"></param>
    protected virtual void PlayAnimation(Tile sourceTile, Tile targetTile, PlayerAttack source)
    {
        if (sourceTile != targetTile) {
            float modelRotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
            source.transform.rotation = Quaternion.Euler(0, modelRotation, 0);
        }

        if (source.TryGetComponent(out Animator animator)) animator.Play(animStateName);

        WaitForAttackEndAction action = new WaitForAttackEndAction(attackDuration, source.gameObject);
        ActionManager.AddToBottom(action);

        foreach (VFXInfo vfxInfo in vfxInfos)
            vfxInfo.Play(action, source.gameObject, targetTile);
    }

    public int Cost                   => cost;
    public string Title               => title;
    public string Description         => description;
    public string EffectDescription   => effectDescription;
    public string RangeDescription    => rangeDescription;
    public string CooldownDescription => cooldownDescription;
    public int Cooldown               => cooldown;
    public int RemainingCooldown      => remainingCooldown;
    public Sprite SkillBarIcon        => skillBarIcon;
    public Sprite InventoryIcon       => inventoryIcon;
    public TileSearch Range           => range;
    public Color Color                => playerColor;
    public WeaponEnum Weapon          => weapon;
    public ArtifactRarity Rarity      => rarity;
}
