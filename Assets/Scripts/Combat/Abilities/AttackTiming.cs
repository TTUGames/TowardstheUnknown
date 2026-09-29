using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// How an attack's clip plays in time: an anticipation slowing into a held pose, a fast swing up to the strike (the impact,
/// or the end of a spell's gesture), a pose held on the strike, then the recovery. Its positions are in seconds of the clip played at the ability's speed, as the impact
/// and the VFX delays are, so that the phases carry them with the pose they were set on. Off, the clip plays at a constant speed
/// </summary>
[Serializable]
public class AttackTiming
{
    [Tooltip("Plays the clip through the phases below; otherwise at a constant speed")]
    public bool enabled;
    [ShowIf("enabled"), Min(0), SuffixLabel("s"), Tooltip("The end of the anticipation, where the swing starts, in seconds of the clip at the ability's speed; the swing ends at the impact")]
    public float swingStart;
    [ShowIf("enabled"), Min(0.05f), Tooltip("Speed of the anticipation")]
    public float windupSpeed = 1;
    [ShowIf("enabled"), Min(0), SuffixLabel("s"), Tooltip("Real seconds the pose is held at the end of the anticipation, which slows into it")]
    public float windupHold;
    [ShowIf("enabled"), Min(0), SuffixLabel("s"), Tooltip("The end of the swing, where its pose is held, in seconds of the clip at the ability's speed; 0 for the impact. A spell's gesture ends before its effect lands")]
    public float strike;
    [ShowIf("enabled"), Min(0.05f), Tooltip("Speed of the swing, up to the strike")]
    public float swingSpeed = 1.6f;
    [ShowIf("enabled"), Min(0), SuffixLabel("s"), Tooltip("Real seconds the pose is held on the strike")]
    public float strikeHold = 0.1f;
    [ShowIf("enabled"), Min(0.05f), Tooltip("Speed of the recovery, which eases out of the impact pose")]
    public float recoverySpeed = 1;
    [ShowIf("enabled"), Min(0), SuffixLabel("s"), Tooltip("Real seconds the next actions wait after the impact while the phases play; 0 for the rest of the ability's duration")]
    public float recovery;

    /// <param name="impact">The impact, in seconds of the clip at the ability's speed: the strike, unless set</param>
    /// <param name="end">The end of the clip, in the same seconds</param>
    /// <param name="active">Whether the edition plays the phases</param>
    public AttackClock Clock(float impact, float end, bool active) =>
        enabled && active && end > 0 ? new AttackClock(this, Mathf.Min(strike > 0 ? strike : impact, end), end) : AttackClock.Linear;
}
