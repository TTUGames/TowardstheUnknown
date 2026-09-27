using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The settings of an edition (Assets/Data/Editions), read through <see cref="Edition.Profile"/> by the systems that differ
/// between the editions without being turned off as a whole. Only settings: a system that needs more is an
/// <see cref="EditionOnly"/> or a pair of the <see cref="EditionSkin"/>
/// </summary>
[CreateAssetMenu(fileName = "EditionProfile", menuName = "TTU/Edition Profile")]
public class EditionProfile : ScriptableObject
{
    [Tooltip("Set on the quality level while the edition is shown; none keeps the quality level's own")]
    public RenderPipelineAsset renderPipeline;

    [BoxGroup("Entities"), Tooltip("A hit flashes the entity white and pushes its model back")]
    public bool hitReactions = true;
    [BoxGroup("Entities"), Tooltip("A dead entity plays its death and shrinks into the ground before it is removed; otherwise it goes at once")]
    public bool deathAnimation = true;
    [BoxGroup("Entities"), Tooltip("The property of the player's outfit materials holding its glow color")]
    public string outfitColorProperty = "_GlowColor";
    [BoxGroup("Entities"), Min(0), Tooltip("Multiplies the neons' color, the outfit's and the weapons' (the original's 3.5); 0 keeps the outfit's as is and the weapons' PlayerGlow intensity")]
    public float neonIntensity;
    [BoxGroup("Entities"), Tooltip("The neons' color at rest, before the intensity; a zero alpha takes the outfit material's")]
    public Color neonRestColor = Color.clear;
    [BoxGroup("Entities"), Tooltip("The outfit's glow follows the energy and the turns, and flashes on a cast")]
    public bool outfitGlowLevel = true;

    [BoxGroup("Board"), Tooltip("Hovering a reachable tile lights the whole path to it, not only the tile")]
    public bool pathPreview = true;
    [BoxGroup("Board"), Tooltip("Hovering an enemy marks the tiles it can hit this turn")]
    public bool threatTiles = true;
    [BoxGroup("Board"), Tooltip("Hovering an entity of the timeline points the board at it: its tile, and a click casts on it")]
    public bool timelinePointsBoard = true;

    [BoxGroup("Input"), Tooltip("Casts clicked during another one are paid and queued; otherwise the input waits for the cast to end")]
    public bool castQueue = true;
    [BoxGroup("Input"), Tooltip("A refused action blinks the tile, shakes the skill or the energy and plays the refusal sound")]
    public bool refusalFeedback = true;
    [BoxGroup("Input"), Tooltip("Ending the turn with a castable artifact, and leaving the run from the pause, ask for a second click")]
    public bool confirmations = true;
    [BoxGroup("Input"), Tooltip("The end turn key presses the action button")]
    public bool endTurnKey = true;

    [BoxGroup("HUD"), Tooltip("The end turn button beats once the energy is spent")]
    public bool endTurnBeat = true;
    [BoxGroup("HUD"), Tooltip("Tooltips on the stats, the status effects and the timeline with the armor, movement and statuses; the skills' with their range and cooldown")]
    public bool detailedTooltips = true;
    [BoxGroup("HUD"), Tooltip("Popups for the armor taking a hit, heals, armor, status effects and the score, the hits adding up and growing with the damage; otherwise one plain number per hit, before the armor")]
    public bool detailedPopups = true;
}
