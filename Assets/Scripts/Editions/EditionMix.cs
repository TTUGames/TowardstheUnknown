using UnityEngine;

/// <summary>
/// Gives Wwise the edition through a global game parameter (0 Anniversary, 1 Classic): in the Classic, the Impacts bus
/// bypasses its ImpactMeter, so that the music is no longer ducked by the hits, as in the original.
/// On the StartSettings object of Managers/Settings.prefab, in the main menu and the game rig
/// </summary>
public class EditionMix : MonoBehaviour
{
    [SerializeField, Tooltip("The Edition game parameter")] private AK.Wwise.RTPC edition = new AK.Wwise.RTPC();

    private void OnEnable()
    {
        Apply(Edition.Current);
        Edition.Changed += Apply;
    }

    private void OnDisable() => Edition.Changed -= Apply;

    private void Apply(GameEdition current)
    {
        if (AkUnitySoundEngine.IsInitialized()) edition.SetGlobalValue(current == GameEdition.Classic ? 1 : 0);
    }
}
