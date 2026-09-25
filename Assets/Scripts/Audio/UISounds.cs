using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The Wwise events of the UI, shared by the screens referencing this asset
/// </summary>
[CreateAssetMenu(fileName = "UISounds", menuName = "TTU/UI Sounds")]
public class UISounds : ScriptableObject
{
    [BoxGroup("Buttons")] public AK.Wwise.Event buttonHover = new AK.Wwise.Event();
    [BoxGroup("Buttons")] public AK.Wwise.Event buttonClick = new AK.Wwise.Event();

    [BoxGroup("HUD")] public AK.Wwise.Event timelineHover = new AK.Wwise.Event();

    [BoxGroup("Inventory")] public AK.Wwise.Event inventoryOpen = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event inventoryClose = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactPick = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactDrop = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactClick = new AK.Wwise.Event();
    [BoxGroup("Inventory")] public AK.Wwise.Event artifactRotate = new AK.Wwise.Event();
}
