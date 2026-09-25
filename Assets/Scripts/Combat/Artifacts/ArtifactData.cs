using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Definition of an artifact, shared by all its instances. Its name is its ID for localization.
/// Use <c>CreateArtifact</c> to get an instance holding the runtime state (cooldown, uses).
/// </summary>
[CreateAssetMenu(fileName = "NewArtifact", menuName = "TTU/Artifact")]
public class ArtifactData : AbilityData
{
    [PropertyOrder(-2), HorizontalGroup("Header", 70), PreviewField(64), HideLabel, Tooltip("Icon in the skill bar")] public Sprite skillBarIcon;
    [PropertyOrder(-2), VerticalGroup("Header/Info")] public ArtifactRarity rarity;

    [PropertyOrder(-1), BoxGroup("Cast"), MinValue(0)] public int cost;
    [PropertyOrder(-1), BoxGroup("Cast"), MinValue(0), Tooltip("0 means unlimited")] public int maximumUsePerTurn = 1;
    [PropertyOrder(-1), BoxGroup("Cast"), MinValue(0)] public int cooldown;

    [BoxGroup("Animation"), Tooltip("Color of the player's neon lights while casting")] public Color playerColor = Color.white;
    [BoxGroup("Animation"), Tooltip("Weapon shown while casting")] public WeaponEnum weapon = WeaponEnum.none;

    [BoxGroup("Inventory"), PreviewField(48)] public Sprite inventoryIcon;
    [BoxGroup("Inventory"), ShapeGrid(5, nameof(inventoryIcon)), Tooltip("Cells occupied in the inventory")] public List<Vector2Int> shape = new List<Vector2Int>() { Vector2Int.zero };

    public Artifact CreateArtifact() => new Artifact(this);
}
