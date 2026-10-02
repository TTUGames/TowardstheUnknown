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

    [BoxGroup("Entities"), Min(-1), Tooltip("Seconds of blend into an attack; negative keeps EntityAnimator's (the original cut straight in: 0)")]
    public float attackBlendIn = -1;
    [BoxGroup("Entities"), Tooltip("The attacks play their clip through the phases of their AttackTiming (anticipation, held poses, fast swing); otherwise at a constant speed, as the original's")]
    public bool attackTiming = true;
    [BoxGroup("Entities"), Tooltip("An attack's sound waits its ability's soundDelay; otherwise it plays as the attack starts, as the original's")]
    public bool attackSoundDelay = true;
    [BoxGroup("Entities"), Tooltip("A humanoid's attacks blend their legs with the stance's as their ability sets (AbilityData.legs); otherwise they play the whole body, as the original's")]
    public bool attackLegs = true;
    [BoxGroup("Entities"), Min(0), Tooltip("Dissolve units per second of a weapon appearing for an attack (the blade grows over about 5.5 units); 0 keeps Dissolving's (the original's 3: whole after about 2 s)")]
    public float weaponAppearSpeed;
    [BoxGroup("Entities"), Min(0), Tooltip("Dissolve units per second of a weapon taken away for the other, which appears once it is gone: the player never holds both but for an artifact wielding both; 0 shows them together, as the original's")]
    public float weaponSwapSpeed;
    [BoxGroup("Entities"), Min(-1), Tooltip("Seconds of blend into a hit; negative keeps EntityAnimator's (the original's 0.25)")]
    public float hitBlendIn = -1;
    [BoxGroup("Entities"), Tooltip("Pushes, pulls and dashes glide without walking; otherwise they are a walk, as the original's")]
    public bool slideMoves = true;
    [BoxGroup("Entities"), Tooltip("A room's enemies spawn facing the deploy tiles, where the player comes in; otherwise they keep their prefab's rotation, as the original's")]
    public bool spawnFacing = true;
    [BoxGroup("Entities"), Tooltip("On a combat's deploy tiles the player faces the middle of the enemies; otherwise it faces west, as the original")]
    public bool deployFacing = true;
    [BoxGroup("Entities"), Min(0.05f), Tooltip("Multiplies the walk clip's speed (the Anniversary's walk plays faster than the original's)")]
    public float walkClipSpeed = 1;
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
    [BoxGroup("Entities"), Tooltip("Casting tints the outfit and the weapons in the artifact's color; otherwise they keep their own and only flash")]
    public bool castTint;
    [BoxGroup("Entities"), Tooltip("The outfit's color goes on instances of its materials, as the original's ChangeColor did, instead of a property block (the original's glow shader renders a block's HDR color much brighter)")]
    public bool outfitColorOnMaterials;

    [BoxGroup("Camera"), Tooltip("The hits shake the camera and freeze the time, and the last kill slows it down and zooms in")]
    public bool impactFeedback = true;
    [BoxGroup("Camera"), Tooltip("A hit taking the player's health plays the original's short sideways shake")]
    public bool playerHitShake;
    [BoxGroup("Camera"), Tooltip("A scene load (menu, game) plays the wipe; otherwise it is a cut, as the original's. The room changes' look is USS (--wipe-plain)")]
    public bool sceneWipe = true;
    [BoxGroup("Camera"), Min(0), Tooltip("Seconds of the victory's beat once a combat ends, before its reward appears and the player moves again; the original's reward was there at once")]
    public float victoryBeat = 1.2f;
    [BoxGroup("Camera"), Min(0), Tooltip("Seconds between an enemy's turn and the next enemy's, so that each reads apart; the original's followed at once")]
    public float enemyTurnGap = 0.18f;
    [BoxGroup("Camera"), Tooltip("A combat's reward grows out of its tile; otherwise it is there at once, as the original's")]
    public bool rewardPopIn = true;

    [BoxGroup("Map"), Tooltip("The plants made of several meshes bend as one in the wind (WindAnchor); the Classic's materials have no wind, and the anchor's property block would only keep the plants out of the SRP Batcher")]
    public bool plantWind = true;

    [BoxGroup("Audio"), Tooltip("Each place has its ambience loop (AmbienceDirector), with a water layer in the rooms with pools; otherwise the original's single ambience plays everywhere")]
    public bool placeAmbience = true;

    [BoxGroup("Board"), Tooltip("Hovering a reachable tile lights the whole path to it, not only the tile")]
    public bool pathPreview = true;
    [BoxGroup("Board"), Tooltip("Hovering an enemy marks the tiles it can hit this turn")]
    public bool threatTiles = true;
    [BoxGroup("Board"), Tooltip("The damage the selected artifact would deal, over each target")]
    public bool damagePreview = true;
    [BoxGroup("Board"), Tooltip("The pointer picks a tile through an entity's model; otherwise the tiles only, as the original")]
    public bool modelPicking = true;
    [BoxGroup("Board"), Tooltip("Hovering an entity of the timeline points the board at it: its tile, and a click casts on it")]
    public bool timelinePointsBoard = true;
    [BoxGroup("Board"), Tooltip("Hovering an entity's model shows its info even when the pointer picks the tiles only (modelPicking off), as the original's")]
    public bool infoOnModelHover;

    [BoxGroup("Input"), Tooltip("Casts clicked during another one are paid and queued; otherwise the input waits for the cast to end")]
    public bool castQueue = true;
    [BoxGroup("Input"), Tooltip("A refused action blinks the tile, shakes the skill or the energy and plays the refusal sound")]
    public bool refusalFeedback = true;
    [BoxGroup("Input"), Tooltip("Ending the turn with a castable artifact, leaving the run from the pause or the results, resetting an options page and closing a chest not emptied ask for a second click")]
    public bool confirmations = true;
    [BoxGroup("Input"), Tooltip("The end turn key presses the action button")]
    public bool endTurnKey = true;

    [BoxGroup("HUD"), Tooltip("Hovering an inventory artifact shows its info; otherwise a press does, as the original")]
    public bool hoverArtifactInfo = true;
    [BoxGroup("HUD"), Tooltip("Opening the inventory shows the first artifact's details; otherwise they stay empty until a piece is pressed, as the original's")]
    public bool prefillArtifactInfo = true;
    [BoxGroup("HUD"), Tooltip("A grabbed inventory piece keeps the point pressed under the pointer; otherwise its origin cell jumps to the pointer, as the original's")]
    public bool grabWhereClicked = true;
    [BoxGroup("HUD"), Min(0), Tooltip("Points the pointer moves on an inventory piece before it is dragged (the original's EventSystem: 10)")]
    public float inventoryDragThreshold = 6;
    [BoxGroup("HUD"), Tooltip("The HUD's, results' and skills' buttons play the button sounds, and the sliders tick")]
    public bool extraUISounds = true;
    [BoxGroup("HUD"), Tooltip("A relic bursts in its rarity's color as it opens, then its pieces come into the chest one by one, the rarest last, each in a flash of its rarity; otherwise the chest opens at once, full, as the original's")]
    public bool chestReveal = true;
    [BoxGroup("HUD"), Tooltip("A turned inventory piece swings to its new orientation")]
    public bool pieceTurnAnimation = true;
    [BoxGroup("HUD"), Tooltip("The end turn button beats once the energy is spent")]
    public bool endTurnBeat = true;
    [BoxGroup("HUD"), Tooltip("Tooltips on the stats, the status effects and the timeline with the armor, movement and statuses; the skills' with their range and cooldown")]
    public bool detailedTooltips = true;
    [BoxGroup("HUD"), Min(0), Tooltip("Milliseconds between the pointer entering a skill and its tooltip showing (the original's 500)")]
    public int skillTooltipDelay = 400;
    [BoxGroup("HUD"), Min(0), Tooltip("Milliseconds between the pointer entering a timeline entity and its tooltip showing (the original's at once)")]
    public int timelineTooltipDelay = 400;
    [BoxGroup("HUD"), Tooltip("The hovered enemy's info writes its name in capitals, as the original's")]
    public bool entityInfoCaps;
    [BoxGroup("HUD"), Min(0), Tooltip("The enemy info's center below the enemy's feet near the top of the screen, in screen heights")]
    public float entityInfoBelow = 0.07f;
    [BoxGroup("HUD"), Tooltip("The enemy info above an enemy takes the enemy's own height (EntityData.classicInfoOffset) and the original's small shift right; otherwise one height for all")]
    public bool entityInfoOriginalAbove;
    [BoxGroup("HUD"), Tooltip("A hit enemy shows its info until the hover changes away from it or it dies, as the original's")]
    public bool infoOnHit;
    [BoxGroup("HUD"), Tooltip("The minimap slides to keep the current room at its center; otherwise the rooms keep the original's fixed 30 point grid")]
    public bool minimapCentered = true;
    [BoxGroup("HUD"), Tooltip("The character sheet writes the health out of the maximum with the armor apart and the scores with their thousands grouped; otherwise the original's \"PV : 100 (0) /100\" and six digit score")]
    public bool readableStats = true;
    [BoxGroup("HUD"), Tooltip("Popups for the armor taking a hit, heals, armor, status effects and the score, the hits adding up and growing with the damage; otherwise one plain number per hit, before the armor")]
    public bool detailedPopups = true;
}
