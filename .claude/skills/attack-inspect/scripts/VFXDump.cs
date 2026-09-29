using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

// Lists the particle systems and VFX graphs of a VFX prefab: timing, size, count, space, material. arg: the prefab path
public static class VFXDump
{
    public static string Run(string path)
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (root == null) return "no prefab " + path;
        var s = new StringBuilder();
        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            var m = ps.main;
            var e = ps.emission;
            var bursts = new ParticleSystem.Burst[e.burstCount];
            e.GetBursts(bursts);
            var r = ps.GetComponent<ParticleSystemRenderer>();
            s.AppendLine($"{AnimationUtility.CalculateTransformPath(ps.transform, root.transform)}: delay {Curve(m.startDelay)} life {Curve(m.startLifetime)} " +
                $"size {Curve(m.startSize)} speed {Curve(m.startSpeed)} dur {m.duration} loop {m.loop} rate {Curve(e.rateOverTime)} " +
                $"bursts [{string.Join(",", bursts.Select(b => $"{b.time}:{Curve(b.count)}"))}] max {m.maxParticles} space {m.simulationSpace} " +
                $"mode {r?.renderMode} mat {r?.sharedMaterial?.name} scale {ps.transform.localScale} pos {ps.transform.localPosition}");
        }
        foreach (var v in root.GetComponentsInChildren<VisualEffect>(true))
            s.AppendLine($"{AnimationUtility.CalculateTransformPath(v.transform, root.transform)}: graph {v.visualEffectAsset?.name}");
        foreach (var l in root.GetComponentsInChildren<Light>(true))
            s.AppendLine($"{AnimationUtility.CalculateTransformPath(l.transform, root.transform)}: light {l.type} {l.intensity} range {l.range} {l.color}");
        return s.ToString();
    }

    private static string Curve(ParticleSystem.MinMaxCurve c) => c.mode switch
    {
        ParticleSystemCurveMode.Constant => c.constant.ToString("0.###"),
        ParticleSystemCurveMode.TwoConstants => $"{c.constantMin:0.###}-{c.constantMax:0.###}",
        _ => $"{c.mode}x{c.curveMultiplier:0.###}"
    };
}
