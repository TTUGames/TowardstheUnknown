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

    [BoxGroup("Inventory"), ShapeGrid(5, nameof(skillBarIcon)), Tooltip("Cells occupied in the inventory")] public List<Vector2Int> shape = new List<Vector2Int>() { Vector2Int.zero };
    [BoxGroup("Inventory"), Tooltip("How the skill bar icon is laid on the piece's largest rectangle, never stretched: whole, or covering it")]
    public ArtifactIconFit inventoryIconFit = ArtifactIconFit.Contain;
    [BoxGroup("Inventory"), Tooltip("Turns the icon on the piece, in degrees clockwise: 0, 90, 180 or 270")] public int inventoryIconRotation;
    [BoxGroup("Inventory"), Tooltip("Moves the icon on the piece, in cells, right and up")] public Vector2 inventoryIconOffset;
    [BoxGroup("Inventory"), Range(0.3f, 1.2f), Tooltip("Size of the icon in its rectangle, 1 touching its sides (whole) or its edges (covering)")] public float inventoryIconScale = 0.85f;
    [BoxGroup("Inventory"), ReadOnly, PiecePreview, Tooltip("Where the drawing lies in the icon's sprite, from 0 to 1 from its bottom left corner: its transparent margin is left out. Set by Tools/Artifacts/Measure Icons")]
    public Rect inventoryIconBounds = new Rect(0, 0, 1, 1);

    public Artifact CreateArtifact() => new Artifact(this);
}
