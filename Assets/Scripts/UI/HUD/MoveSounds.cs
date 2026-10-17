using System;
using UnityEngine;

/// <summary>
/// Plays a soft tick when the player's click starts a move (<see cref="PlayerMove.MoveConfirmed"/>), in an edition with
/// <see cref="EditionProfile.extraUISounds"/>: the original's moves were silent
/// </summary>
public class MoveSounds : IDisposable
{
    private readonly PlayerMove move;
    private readonly GameObject soundEmitter;
    private readonly UISounds sounds;

    public MoveSounds(PlayerMove move, GameObject soundEmitter, UISounds sounds)
    {
        this.move = move;
        this.soundEmitter = soundEmitter;
        this.sounds = sounds;
        move.MoveConfirmed += Play;
    }

    public void Dispose()
    {
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (move != null) move.MoveConfirmed -= Play;
    }

    private void Play()
    {
        if (Edition.Profile.extraUISounds) sounds.moveConfirm.Post(soundEmitter);
    }
}
