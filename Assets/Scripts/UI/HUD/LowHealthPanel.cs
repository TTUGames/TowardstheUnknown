using System;
using UnityEngine.UIElements;

/// <summary>
/// A faint red vignette around the screen while the player's health is low: it beats a few times when the health falls
/// under the threshold and when the player is hit meanwhile, then stays dim, so that it warns without nagging
/// </summary>
public class LowHealthPanel : IDisposable
{
    private const int Beats = 3;
    private const long BeatOn = 160;
    // The cycle matches the heartbeat's loop (750 ms)
    private const long BeatOff = 590;
    private const long BeatTick = 20;

    private readonly VisualElement vignette;
    private readonly PlayerStats stats;
    private bool low;
    private IVisualElementScheduledItem beating;
    private float beatStart;

    public LowHealthPanel(VisualElement vignette, PlayerStats stats)
    {
        this.vignette = vignette;
        this.stats = stats;
        stats.StatsChanged += Refresh;
        GameEvents.DamageTaken += OnDamageTaken;
        Refresh();
    }

    public void Dispose()
    {
        GameEvents.DamageTaken -= OnDamageTaken;
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (stats != null) stats.StatsChanged -= Refresh;
    }

    private void Refresh()
    {
        bool isLow = stats.IsHealthLow;
        if (isLow == low) return;
        low = isLow;
        vignette.EnableInClassList("low-health--shown", low);
        if (low) Beat();
        else StopBeating();
    }

    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        // Raised before the health goes down: Refresh follows on the stats' change
        if (entity == stats && healthLost > 0 && low) Beat();
    }

    private void Beat()
    {
        beatStart = UnityEngine.Time.unscaledTime;
        beating ??= vignette.schedule.Execute(UpdateBeat).Every(BeatTick);
    }

    // On for BeatOn, off for BeatOff, Beats times, then dim until the next beat
    private void UpdateBeat()
    {
        float elapsed = (UnityEngine.Time.unscaledTime - beatStart) * 1000;
        float cycle = BeatOn + BeatOff;
        bool done = !low || elapsed >= Beats * cycle;
        vignette.EnableInClassList("low-health--beat", !done && elapsed % cycle < BeatOn);
        if (!done) return;
        beating.Pause();
        beating = null;
    }

    private void StopBeating()
    {
        beating?.Pause();
        beating = null;
        vignette.RemoveFromClassList("low-health--beat");
    }
}
