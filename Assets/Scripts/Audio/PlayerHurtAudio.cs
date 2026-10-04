using System.Collections;
using UnityEngine;

/// <summary>
/// Muffles the music when the player is hit, the more the harder the hit, and beats a heart while their health is low.
/// Both drive global Wwise game parameters that Wwise smooths and maps to the music bus's low-pass. The low health's muffle
/// and heartbeat follow the LowHealthAudio setting. At the player's death the heart beats its last beat and stops, the
/// music staying muffled until the results
/// </summary>
public class PlayerHurtAudio : MonoBehaviour
{
    // Muffle of a hit the armor took whole, then of the lightest and of the heaviest hit on the health
    private const float ArmorHurt = 15;
    private const float MinHurt = 40;
    private const float MaxHurt = 100;
    // Share of the maximum health a hit costs to muffle the most
    private const float HeavyHitShare = 0.3f;

    [SerializeField] private PlayerStats stats;
    [SerializeField, Tooltip("0 to 100: how muffled the music is, eased back by Wwise")] private AK.Wwise.RTPC hurt = new AK.Wwise.RTPC();
    [SerializeField, Tooltip("0 while the health isn't low, then 50 at the threshold up to 100 at the last point")] private AK.Wwise.RTPC lowHealth = new AK.Wwise.RTPC();
    [SerializeField, Tooltip("Loops while the health is low")] private AK.Wwise.Event heartbeat = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event heartbeatStop = new AK.Wwise.Event();
    [SerializeField, Tooltip("Seconds a hit's muffle holds before easing back")] private float hold = 0.18f;

    private bool beating;
    private Coroutine release;

    private void OnEnable()
    {
        GameEvents.DamageTaken += OnDamageTaken;
        stats.StatsChanged += Refresh;
        GameTime.PausedChanged += OnPausedChanged;
        GameSettings.Changed += OnSettingChanged;
        GameEvents.RunEnded += OnRunEnded;
        Refresh();
    }

    private void OnSettingChanged(GameSetting setting)
    {
        if (setting == GameSetting.LowHealthAudio) Refresh();
    }

    // The results bring the music back
    private void OnRunEnded(bool isVictory)
    {
        if (AkUnitySoundEngine.IsInitialized()) lowHealth.SetGlobalValue(0);
    }

    // The heart stops beating behind the pause menu, and picks up where it was
    private void OnPausedChanged(bool paused)
    {
        if (!beating || !AkUnitySoundEngine.IsInitialized()) return;
        heartbeat.ExecuteAction(gameObject, paused ? AkActionOnEventType.AkActionOnEventType_Pause : AkActionOnEventType.AkActionOnEventType_Resume,
            0, AkCurveInterpolation.AkCurveInterpolation_Linear);
    }

    private void OnDisable()
    {
        GameEvents.DamageTaken -= OnDamageTaken;
        stats.StatsChanged -= Refresh;
        GameTime.PausedChanged -= OnPausedChanged;
        GameSettings.Changed -= OnSettingChanged;
        GameEvents.RunEnded -= OnRunEnded;
        release = null;
        // The game parameters are global: they would outlive the player
        if (!AkUnitySoundEngine.IsInitialized()) return;
        hurt.SetGlobalValue(0);
        lowHealth.SetGlobalValue(0);
        if (beating) heartbeat.Stop(gameObject);
        beating = false;
    }

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        if (entity != stats || damage <= 0) return;
        float share = Mathf.Clamp01(healthLost / (stats.MaxHealth * HeavyHitShare));
        hurt.SetGlobalValue(healthLost > 0 ? Mathf.Lerp(MinHurt, MaxHurt, share) : ArmorHurt);
        if (release != null) StopCoroutine(release);
        release = StartCoroutine(Release());
    }

    private IEnumerator Release()
    {
        yield return new WaitForSecondsRealtime(hold);
        hurt.SetGlobalValue(0);
        release = null;
    }

    private void Refresh()
    {
        if (stats.IsDead)
        {
            Die();
            return;
        }
        // Off in the settings: neither muffle nor heartbeat
        bool low = stats.IsHealthLow && GameSettings.Get(GameSetting.LowHealthAudio) > 0;
        float threshold = stats.MaxHealth * PlayerStats.LowHealthShare;
        lowHealth.SetGlobalValue(low ? 50 + 50 * (1 - stats.CurrentHealth / threshold) : 0);
        if (low == beating) return;
        beating = low;
        (low ? heartbeat : heartbeatStop).Post(gameObject);
    }

    // The heart's loop ends after its beat (a Wwise Break) rather than fading as on a heal, and the music stays muffled at
    // its lowest; the run's end lifts it
    private void Die()
    {
        if (!AkUnitySoundEngine.IsInitialized()) return;
        // Off in the settings: no muffle, and the heart wasn't beating
        if (GameSettings.Get(GameSetting.LowHealthAudio) > 0) lowHealth.SetGlobalValue(100);
        if (!beating) return;
        beating = false;
        heartbeat.ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Break, 0, AkCurveInterpolation.AkCurveInterpolation_Linear);
    }
}
