using UnityEngine;

/// <summary>
/// Gives Wwise the edition through a global game parameter (0 Anniversary, 1 Classic): in the Classic, the Impacts bus
/// bypasses its ImpactMeter, so that the music is no longer ducked by the hits, as in the original; and through the
/// Edition state, which picks the edition's sound in the containers holding one for each.
/// On the StartSettings object of Managers/SETTINGS.prefab, in the main menu and the game rig
/// </summary>
public class EditionMix : MonoBehaviour
{
    [SerializeField, Tooltip("The Edition game parameter")] private AK.Wwise.RTPC edition = new AK.Wwise.RTPC();
    [SerializeField, Tooltip("The Edition state of the Anniversary")] private AK.Wwise.State anniversary = new AK.Wwise.State();
    [SerializeField, Tooltip("The Edition state of the Classic")] private AK.Wwise.State classic = new AK.Wwise.State();

    private void OnEnable()
    {
        Apply(Edition.Current);
        Edition.Changed += Apply;
    }

    private void OnDisable() => Edition.Changed -= Apply;

    private void Apply(GameEdition current)
    {
        if (!AkUnitySoundEngine.IsInitialized()) return;
        edition.SetGlobalValue(current == GameEdition.Classic ? 1 : 0);
        AK.Wwise.State state = current == GameEdition.Classic ? classic : anniversary;
        if (state.IsValid()) state.SetValue();
    }
}
