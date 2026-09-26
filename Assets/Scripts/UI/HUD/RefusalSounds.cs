using System;
using UnityEngine;

/// <summary>
/// Plays the refusal sound when the player selects an artifact it can't cast or clicks the board out of reach or range
/// </summary>
public class RefusalSounds : IDisposable
{
    private readonly PlayerTurn player;
    private readonly GameObject soundEmitter;
    private readonly UISounds sounds;

    public RefusalSounds(PlayerTurn player, GameObject soundEmitter, UISounds sounds)
    {
        this.player = player;
        this.soundEmitter = soundEmitter;
        this.sounds = sounds;
        player.ClickRefused += Play;
        player.playerAttack.ArtifactRefused += OnArtifactRefused;
    }

    public void Dispose()
    {
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (player == null) return;
        player.ClickRefused -= Play;
        player.playerAttack.ArtifactRefused -= OnArtifactRefused;
    }

    private void OnArtifactRefused(Artifact artifact) => Play();

    // The Wwise project has no refusal event yet: the field may be empty
    private void Play()
    {
        if (sounds.refused.IsValid()) sounds.refused.Post(soundEmitter);
    }
}
