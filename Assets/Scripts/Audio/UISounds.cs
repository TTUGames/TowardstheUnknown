using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// The Wwise events of the UI, shared by the screens referencing this asset
/// </summary>
[CreateAssetMenu(fileName = "UISounds", menuName = "TTU/UI Sounds")]
public class UISounds : ScriptableObject
{
    [BoxGroup("Buttons")] public AK.Wwise.Event buttonHover = new AK.Wwise.Event();
    [BoxGroup("Buttons")] public AK.Wwise.Event buttonClick = new AK.Wwise.Event();

    [BoxGroup("HUD")] public AK.Wwise.Event timelineHover = new AK.Wwise.Event();
    [BoxGroup("HUD"), Tooltip("An artifact that can't be cast, a click on the board out of reach or range")] public AK.Wwise.Event refused = new AK.Wwise.Event();
    [BoxGroup("HUD"), Tooltip("The combat starting: the deploy button and the Combat banner in one sound, posted as the banner slides in")]
    [FormerlySerializedAs("bannerCombat")] public AK.Wwise.Event combatStart = new AK.Wwise.Event();
    [BoxGroup("HUD"), Tooltip("The turns' and the victory's banners as they slide in")]
    public AK.Wwise.Event bannerPlayerTurn = new AK.Wwise.Event();
    [BoxGroup("HUD")] public AK.Wwise.Event bannerEnemyTurn = new AK.Wwise.Event();
    [BoxGroup("HUD")] public AK.Wwise.Event bannerVictory = new AK.Wwise.Event();
    [BoxGroup("HUD"), Tooltip("The room changes' wipe covering the screen, then uncovering it")] public AK.Wwise.Event wipeCover = new AK.Wwise.Event();
    [BoxGroup("HUD")] public AK.Wwise.Event wipeReveal = new AK.Wwise.Event();

    [BoxGroup("Deploy"), Tooltip("The pointer comes on a deploy tile")] public AK.Wwise.Event deployHover = new AK.Wwise.Event();
    [BoxGroup("Deploy"), Tooltip("The player is placed on the clicked deploy tile")] public AK.Wwise.Event deploySelect = new AK.Wwise.Event();

    [BoxGroup("Pause")] public AK.Wwise.Event pauseOpen = new AK.Wwise.Event();
    [BoxGroup("Pause")] public AK.Wwise.Event pauseClose = new AK.Wwise.Event();

    [BoxGroup("Inventory")] public AK.Wwise.Event inventoryOpen = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event inventoryClose = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactPick = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactDrop = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactClick = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactRotate = new AK.Wwise.Event();
    [BoxGroup("Inventory"), Tooltip("A piece dropped where it doesn't fit, shaking back home (EditionProfile.refusalFeedback)")] public AK.Wwise.Event artifactRefused = new AK.Wwise.Event();
}
