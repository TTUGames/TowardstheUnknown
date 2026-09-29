using System.IO;
using UnityEditor;
using UnityEngine;

// Builds the Anniversary VFX of a player's attack: copies the current prefab as its Classic side, then a new Anniversary prefab
// from it with a play rate and impact sparks. spec: "name,playRate,sparksDelay,sparksCount,r,g,b,forward[,up]"
public static class AttackVFXBuild
{
    public static string Run(string spec)
    {
        string[] args = spec.Split(',');
        string name = args[0];
        float playRate = float.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture);
        float delay = float.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture);
        int count = int.Parse(args[3]);
        var color = new Color(F(args[4]), F(args[5]), F(args[6]), 1);
        float forward = F(args[7]);
        float up = args.Length > 8 ? F(args[8]) : 0;

        string source = $"Assets/Prefabs/VFX/{name}.prefab";
        string anniversary = $"Assets/Prefabs/VFX/Attacks/{name}.prefab";
        string classic = $"Assets/Art/Classic/Prefabs/VFX/Attacks/{name}_Classic.prefab";
        Directory.CreateDirectory(Path.GetDirectoryName(anniversary));
        Directory.CreateDirectory(Path.GetDirectoryName(classic));
        AssetDatabase.Refresh();
        if (!File.Exists(classic) && !AssetDatabase.CopyAsset(source, classic)) return "copy classic failed";
        if (!File.Exists(anniversary) && !AssetDatabase.CopyAsset(source, anniversary)) return "copy anniversary failed";

        GameObject root = PrefabUtility.LoadPrefabContents(anniversary);
        root.name = name;
        var rate = root.GetComponent<VFXPlayRate>();
        if (rate == null) rate = root.AddComponent<VFXPlayRate>();
        rate.playRate = playRate;
        Transform old = root.transform.Find("ImpactSparks");
        if (old != null) Object.DestroyImmediate(old.gameObject);
        if (count > 0) Sparks(root.transform, delay, count, color, forward, up);
        PrefabUtility.SaveAsPrefabAsset(root, anniversary);
        PrefabUtility.UnloadPrefabContents(root);
        return $"built {anniversary} (classic {classic})";
    }

    private static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

    private static void Sparks(Transform parent, float delay, int count, Color color, float forward, float up)
    {
        var go = new GameObject("ImpactSparks");
        go.transform.SetParent(parent, false);
        // A VFX root faces away from its target (VFXInfo turns it from the target tile to the source): the sparks sit towards the
        // target and fly away from the attacker
        go.transform.SetLocalPositionAndRotation(new Vector3(0, up, -forward), Quaternion.Euler(0, 180, 0));
        // At its own time, whatever the rate of the effect it is added to
        go.AddComponent<VFXPlayRate>();
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.playOnAwake = true;
        main.startDelay = delay;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.055f);
        main.startColor = new ParticleSystem.MinMaxGradient(color, Color.Lerp(color, Color.white, 0.6f));
        main.gravityModifier = 1.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = count + 4;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0, (short)count) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 55;
        shape.radius = 0.08f;
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0)));
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.dampen = 0.25f;
        limit.limit = 2;
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1) });
        fade.color = gradient;
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = 0.035f;
        renderer.lengthScale = 1.5f;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/VFX/Death/DeathSpark.mat");
    }
}
