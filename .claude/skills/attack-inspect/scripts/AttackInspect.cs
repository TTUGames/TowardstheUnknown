using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Measures the attack clips of the abilities on the player's rig, outside Play mode: the speed of the weapon's tip (a sword
/// attack) or of the fastest hand, the strike (its peak), the cocked pose before it (the slowest moment before the strike),
/// and how the ability's timing plays them. Run through the unity CLI (run_script); nothing is saved.
/// </summary>
public static class AttackInspect
{
    private const float Rate = 120;
    private const int Width = 60;

    /// <param name="filter">Names of abilities, separated by commas (a part of the name is enough); empty for all</param>
    public static string Run(string filter)
    {
        var names = string.IsNullOrWhiteSpace(filter) ? new string[0] : filter.Split(',').Select(n => n.Trim()).ToArray();
        var sb = new StringBuilder();
        GameObject root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Entities/Player.prefab");
        try
        {
            Animator animator = root.GetComponentInChildren<Animator>();
            PlayerAttack attack = root.GetComponent<PlayerAttack>();
            Transform body = animator.transform;
            float scale = body.lossyScale.y;
            Transform sword = attack.Anchor(VFXInfo.Target.SWORD), gun = attack.Anchor(VFXInfo.Target.GUN);
            Transform left = animator.GetBoneTransform(HumanBodyBones.LeftHand), right = animator.GetBoneTransform(HumanBodyBones.RightHand);

            foreach (string guid in AssetDatabase.FindAssets("t:AbilityData", new[] { "Assets/Data" }))
            {
                var data = AssetDatabase.LoadAssetAtPath<AbilityData>(AssetDatabase.GUIDToAssetPath(guid));
                if (names.Length > 0 && !names.Any(n => data.name.IndexOf(n, System.StringComparison.OrdinalIgnoreCase) >= 0)) continue;
                AnimationClip clip = data.animationClip;
                if (clip == null) { sb.AppendLine($"{data.name}: no clip"); continue; }
                float speed = Mathf.Max(0.05f, data.animationSpeed);
                bool swordAttack = data is ArtifactData artifact && artifact.weapon == WeaponEnum.sword;
                string effector = swordAttack ? "sword tip" : "hands";
                sb.AppendLine($"== {data.name}  clip {clip.name} ({clip.length / speed:0.00} s at x{speed}), duration {data.duration}, impact {data.impactDelay}, VFX {string.Join(" ", data.vfx.Select(v => v.Prefab ? v.Prefab.name : "none"))}");
                if (!clip.humanMotion) { sb.AppendLine("   generic rig: not sampled on the player"); continue; }

                //Speeds in body heights per second, per sample of the clip at the ability's speed
                var speeds = new List<float>();
                Vector3[] last = null;
                int count = Mathf.CeilToInt(clip.length * Rate);
                for (int i = 0; i <= count; i++)
                {
                    clip.SampleAnimation(body.gameObject, Mathf.Min(i / Rate, clip.length));
                    Transform[] points = swordAttack ? new[] { sword } : new[] { left, right, gun };
                    Vector3[] now = points.Select(p => body.InverseTransformPoint(p.position)).ToArray();
                    if (last != null) speeds.Add(now.Zip(last, (a, b) => (a - b).magnitude).Max() * Rate * speed);
                    last = now;
                }
                float Seconds(int sample) => (sample + 1) / Rate / speed;
                //The strike: the fastest moment of the first 85 % of the clip
                int strike = 0;
                for (int i = 0; i < speeds.Count * 0.85f; i++) if (speeds[i] > speeds[strike]) strike = i;
                //The cocked pose: the slowest moment within 0.6 s before the strike
                int cocked = strike;
                for (int i = strike; i >= 0 && Seconds(strike) - Seconds(i) <= 0.6f; i--) if (speeds[i] < speeds[cocked]) cocked = i;
                //Still again: the first moment after the strike under 15 % of its speed
                int still = strike;
                while (still < speeds.Count - 1 && speeds[still] > speeds[strike] * 0.15f) still++;
                sb.AppendLine($"   {effector}: cocked {Seconds(cocked):0.00}  strike {Seconds(strike):0.00} (peak {speeds[strike]:0.0})  still {Seconds(still):0.00}");
                sb.AppendLine("   speed  " + Spark(speeds, s => s));
                sb.AppendLine("   marks  " + Marks(speeds.Count, (Seconds(cocked), 'c'), (Seconds(strike), 'S'), (data.impactDelay, 'I'), (Seconds(still), '.'), speed));

                AttackTiming timing = data.timing;
                float end = clip.length / speed;
                AttackClock clock = timing.Clock(data.impactDelay, end, true);
                if (!timing.enabled) { sb.AppendLine($"   timing off: impact at {data.impactDelay:0.00} s, clip ends at {end:0.00} s"); continue; }
                sb.AppendLine($"   timing: swing from {timing.swingStart} (hold {timing.windupHold}, x{timing.windupSpeed} before), swing x{timing.swingSpeed} to {(timing.strike > 0 ? timing.strike : data.impactDelay)}, strike hold {timing.strikeHold}, recovery x{timing.recoverySpeed}");
                sb.AppendLine($"   real:   swing at {clock.TimeAt(timing.swingStart):0.00} s, strike at {clock.TimeAt(timing.strike > 0 ? timing.strike : data.impactDelay):0.00} s, impact at {clock.EventTime(data.impactDelay):0.00} s, clip ends at {clock.TimeAt(end):0.00} s (duration {data.duration})");
                //The clip's speed in real time: how fast its position moves under the timing
                int realCount = Mathf.CeilToInt(clock.TimeAt(end) * Rate);
                var warped = new List<float>();
                for (int i = 0; i < realCount; i++) warped.Add(clock.Position((i + 1) / Rate) - clock.Position(i / Rate));
                sb.AppendLine("   played " + Spark(warped, s => s));
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        return sb.ToString();
    }

    private static string Spark(List<float> values, System.Func<float, float> map)
    {
        const string levels = " ▁▂▃▄▅▆▇█";
        if (values.Count == 0) return "";
        float max = values.Max(map);
        var sb = new StringBuilder();
        for (int c = 0; c < Width; c++)
        {
            int from = c * values.Count / Width, to = Mathf.Max(from + 1, (c + 1) * values.Count / Width);
            float v = 0;
            for (int i = from; i < to && i < values.Count; i++) v = Mathf.Max(v, map(values[i]));
            sb.Append(levels[Mathf.Clamp(Mathf.RoundToInt(v / Mathf.Max(max, 1e-4f) * 8), 0, 8)]);
        }
        return sb.ToString();
    }

    private static string Marks(int samples, (float time, char mark) a, (float time, char mark) b, (float time, char mark) c, (float time, char mark) d, float speed)
    {
        var line = new char[Width];
        for (int i = 0; i < Width; i++) line[i] = '·';
        float length = samples / Rate / speed;
        foreach (var (time, mark) in new[] { a, d, b, c })
            line[Mathf.Clamp((int)(time / length * Width), 0, Width - 1)] = mark;
        return new string(line);
    }
}
