using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack's timing played out: the position in its clip (seconds at the ability's speed) at each real second since its start, and back.
/// Past the end of the clip, the position goes on at a constant speed (a follow-up clip)
/// </summary>
public class AttackClock
{
    public static readonly AttackClock Linear = new AttackClock();

    // Real seconds between two positions
    private const float Step = 1 / 240f;
    // Speed of a held pose: it breathes rather than freezes
    private const float HeldSpeed = 0.06f;
    // Real seconds of the swing's acceleration and of the recovery's
    private const float SwingRamp = 0.04f;
    private const float RecoveryRamp = 0.15f;
    // Seconds of the anticipation, at its speed, slowing into its held pose, and the fraction of its speed it slows to
    private const float WindupEase = 0.08f;
    private const float WindupEaseSpeed = 0.3f;
    private const int MaxSteps = 60 * 240;

    private readonly float[] positions;
    // The end of the swing, in position and in real seconds: what comes after it (a spell flying to its target) keeps its delay
    private readonly float strike;
    private readonly float strikeTime;

    private AttackClock() { }

    public AttackClock(AttackTiming timing, float strike, float end)
    {
        var list = new List<float> { 0 };
        float swingStart = Mathf.Clamp(timing.swingStart, 0, strike);
        float position = 0, speed = timing.windupSpeed, held = 0;
        //0 anticipation, 1 held before the swing, 2 swing, 3 held on the strike, 4 recovery
        int phase = swingStart > 0 ? 0 : 2;
        if (phase == 2) speed = timing.swingSpeed;
        while (position < end && list.Count < MaxSteps)
        {
            switch (phase)
            {
                case 0:
                    if (timing.windupHold > 0)
                    {
                        float ease = Mathf.Clamp01((swingStart - position) / (timing.windupSpeed * WindupEase));
                        speed = timing.windupSpeed * Mathf.Lerp(WindupEaseSpeed, 1, ease);
                    }
                    break;
                case 1:
                    speed = HeldSpeed;
                    break;
                case 2:
                    speed = Mathf.MoveTowards(speed, timing.swingSpeed, Mathf.Abs(timing.swingSpeed - HeldSpeed) * Step / SwingRamp);
                    break;
                case 3:
                    speed = HeldSpeed;
                    break;
                default:
                    speed = Mathf.MoveTowards(speed, timing.recoverySpeed, Mathf.Abs(timing.recoverySpeed - HeldSpeed) * Step / RecoveryRamp);
                    break;
            }
            position += speed * Step;
            list.Add(Mathf.Min(position, end));
            if (phase == 0 && position >= swingStart) phase = timing.windupHold > 0 ? 1 : 2;
            else if (phase == 1 && (held += Step) >= timing.windupHold) phase = 2;
            else if (phase == 2 && position >= strike)
            {
                phase = timing.strikeHold > 0 ? 3 : 4;
                held = 0;
            }
            else if (phase == 3 && (held += Step) >= timing.strikeHold) phase = 4;
        }
        positions = list.ToArray();
        this.strike = strike;
        strikeTime = TimeAt(strike);
    }

    private float Length => (positions.Length - 1) * Step;

    /// <summary>
    /// The position in the clip, in seconds at the ability's speed, the real seconds since the start
    /// </summary>
    public float Position(float time)
    {
        if (positions == null) return time;
        if (time >= Length) return positions[^1] + time - Length;
        float index = Mathf.Max(0, time) / Step;
        int i = (int)index;
        return Mathf.Lerp(positions[i], positions[i + 1], index - i);
    }

    /// <summary>
    /// The real seconds since the start at which an event set at the position happens (the impact, a VFX): with the pose up to
    /// the strike, then as long after it as it was set, whatever the held pose and the recovery
    /// </summary>
    public float EventTime(float position) => positions == null ? position : position <= strike ? TimeAt(position) : strikeTime + position - strike;

    /// <summary>
    /// The real seconds since the start at which the clip reaches the position
    /// </summary>
    public float TimeAt(float position)
    {
        if (positions == null) return position;
        if (position >= positions[^1]) return Length + position - positions[^1];
        if (position <= 0) return 0;
        int low = 0, high = positions.Length - 1;
        while (high - low > 1)
        {
            int middle = (low + high) / 2;
            if (positions[middle] < position) low = middle;
            else high = middle;
        }
        float span = positions[high] - positions[low];
        return (low + (span > 0 ? (position - positions[low]) / span : 0)) * Step;
    }
}
