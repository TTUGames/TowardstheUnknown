using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Definition of an artifact, shared by all its instances. Its name is its ID for localization, animations and sounds.
/// Use <c>CreateArtifact</c> to get an instance holding the runtime state (cooldown, uses).
/// </summary>
[CreateAssetMenu(fileName = "NewArtifact", menuName = "TTU/Artifact")]
public class ArtifactData : ScriptableObject
{
    public enum TargetTag { Enemy, Player }

    [HorizontalGroup("Header", 70), PreviewField(64), HideLabel] public Sprite skillBarIcon;
    [VerticalGroup("Header/Info")] public ArtifactRarity rarity;
    [VerticalGroup("Header/Info")] public Color playerColor = Color.white;
    [VerticalGroup("Header/Info")] public WeaponEnum weapon = WeaponEnum.none;
    [VerticalGroup("Header/Info"), PreviewField(32)] public Sprite inventoryIcon;

    [BoxGroup("Cast"), MinValue(0)] public int cost;
    [BoxGroup("Cast"), MinValue(0), Tooltip("0 means unlimited")] public int maximumUsePerTurn = 1;
    [BoxGroup("Cast"), MinValue(0)] public int cooldown;
    [BoxGroup("Cast"), MinValue(0), SuffixLabel("s")] public float attackDuration = 2f;

    [BoxGroup("Targeting")] public TargetTag target = TargetTag.Enemy;
    [BoxGroup("Targeting")] public TileSearchConfig range = new TileSearchConfig(TileSearchConfig.Shape.CircleAttack, 1, 1);
    [BoxGroup("Targeting"), Tooltip("Hits every entity in an area around the targeted tile instead of the targeted entity")] public bool isAreaOfEffect;
    [BoxGroup("Targeting"), ShowIf("isAreaOfEffect")] public TileSearchConfig area = new TileSearchConfig(TileSearchConfig.Shape.Circle, 0, 1);

    [BoxGroup("Effects"), Tooltip("Applied once, with the caster as target, whatever the number of targets")]
    [SerializeReference, ListDrawerSettings(ShowFoldout = false)] public List<CombatEffect> castEffects = new List<CombatEffect>();
    [BoxGroup("Effects"), Tooltip("Applied to each target, in order")]
    [SerializeReference, ListDrawerSettings(ShowFoldout = false)] public List<CombatEffect> effects = new List<CombatEffect>();

    [BoxGroup("Presentation")] public List<VFXInfo> vfx = new List<VFXInfo>();
    [BoxGroup("Presentation"), Tooltip("Cells occupied in the inventory")] public List<Vector2Int> shape = new List<Vector2Int>() { Vector2Int.zero };

    /// <summary>
    /// The values inserted in the localized effect description: the cast effects' ones, then the per-target effects' ones
    /// </summary>
    public object[] DescriptionValues => castEffects.Concat(effects).Where(effect => effect != null).SelectMany(effect => effect.DescriptionValues).ToArray();

    public Artifact CreateArtifact() => new DataArtifact(this);
}
