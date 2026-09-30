using System.Globalization;
using UnityEditor;
using UnityEngine;

// Anchors an attack's Anniversary VFX (Prefabs/VFX/Attacks/<name>) in the world: adds (or rebuilds) its "Ground" child, a mark
// left on the ground that fades out, a glow cooling on it, a ring of light where the effect meets the ground, dust thrown up and
// a brief light on the floor, and a VFXLifetime keeping the instance until the mark is gone. The Classic's prefab (_Classic) is
// left as is.
// spec: "name,style,radius,r,g,b,start,life[,dust[,light[,dark]]]": style crack | glyph | sigil | stain (the texture of the mark),
// radius of the mark in meters, the element's color, start the real seconds from the play of the VFX to its impact, life the
// mark's seconds (3 to 6), dust the count of dust puffs (0: none), light the flash's intensity (0: none), dark how dark the mark is
public static class AttackGroundBuild
{
    private const string Folder = "Assets/Art/VFX/Ground/";
    // A tile's origin is half a meter under its surface: the mark lies just above it
    private const float TileTop = 0.53f;

    public static string Run(string spec)
    {
        string[] a = spec.Split(',');
        string name = a[0], style = a[1];
        float radius = F(a[2]);
        var color = new Color(F(a[3]), F(a[4]), F(a[5]), 1);
        float start = F(a[6]), life = F(a[7]);
        int dust = a.Length > 8 ? int.Parse(a[8]) : 12;
        float light = a.Length > 9 ? F(a[9]) : 4;
        float dark = a.Length > 10 ? F(a[10]) : 0.6f;

        string path = $"Assets/Prefabs/VFX/Attacks/{name}.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) return "no prefab " + path;
        Transform old = root.transform.Find("Ground");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var ground = new GameObject("Ground").transform;
        ground.SetParent(root.transform, false);
        // On the tile's surface (TileTop above its origin) whatever the VFX's offset (read from the artifact) and the root's scale
        Vector3 scale = root.transform.localScale;
        ground.localScale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);
        ground.localPosition = new Vector3(0, (TileTop - Offset(name)) / scale.y, 0);
        // At its own time, whatever the rate of the effect it is added to
        ground.gameObject.AddComponent<VFXPlayRate>();

        bool stain = style == "stain";
        string mark = style switch { "glyph" => "Glyph", "sigil" => "Sigil", _ => "Crack" };
        // A dark stain under the mark: burnt, wet, bloody ground
        Color under = stain ? Color.Lerp(Color.black, color, 0.35f) : Color.Lerp(Color.black, color, 0.12f);
        Flat(ground, "Stain", "Stain", start, life, radius * 2.3f, under, dark, 0.03f, 0.55f, 1);
        if (!stain)
        {
            Flat(ground, "Mark", mark, start, life, radius * 2, Color.Lerp(Color.black, color, 0.15f), 1, 0.03f, 0.6f, 2);
            // The mark glows with the element at the impact, then cools down
            Flat(ground, "MarkGlow", mark + "Glow", start, Mathf.Min(1.4f, life * 0.35f), radius * 2, color, 1, 0.02f, 0.15f, 3);
        }
        Ring(ground, start, radius, color);
        if (dust > 0) Dust(ground, start, radius, dust);
        if (light > 0) Flash(ground, start, radius, color, light);

        VFXLifetime lifetime = root.GetComponent<VFXLifetime>();
        if (lifetime == null) lifetime = root.AddComponent<VFXLifetime>();
        lifetime.seconds = start + life + 0.1f;
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        return $"ground {style} r{radius} on {path}, kept {lifetime.seconds:F2} s";
    }

    private static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);

    // The VFX offset's height set on the artifact, the root sitting that high above the tile
    private static float Offset(string name)
    {
        var data = AssetDatabase.LoadAssetAtPath<AbilityData>($"Assets/Data/Artifacts/{name}.asset");
        if (data == null) return 0;
        var so = new SerializedObject(data);
        SerializedProperty vfx = so.FindProperty("vfx");
        return vfx != null && vfx.arraySize > 0 ? vfx.GetArrayElementAtIndex(0).FindPropertyRelative("offset").vector3Value.y : 0;
    }

    private static ParticleSystem Make(Transform parent, string name, float delay, float lifetime, int max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 1;
        main.loop = false;
        main.playOnAwake = true;
        main.startDelay = delay;
        main.startLifetime = lifetime;
        main.startSpeed = 0;
        main.maxParticles = max;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        var shape = ps.shape;
        shape.enabled = false;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0, (short)max) });
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return ps;
    }

    private static Gradient Fade(Color color, float alpha, float fadeIn, float hold)
    {
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(alpha, fadeIn), new GradientAlphaKey(alpha, hold), new GradientAlphaKey(0, 1) });
        return gradient;
    }

    // A decal lying on the ground: fades in at the impact, stays, then fades out over the rest of its life
    private static void Flat(Transform parent, string name, string material, float delay, float life, float size, Color color, float alpha, float fadeIn, float hold, int order)
    {
        ParticleSystem ps = Make(parent, name, delay, life, 1);
        var main = ps.main;
        main.startSize = size;
        main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
        main.startColor = Color.white;
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        fade.color = Fade(color, alpha, fadeIn / life, hold);
        // Slams in slightly bigger, then settles
        var grow = ps.sizeOverLifetime;
        grow.enabled = true;
        grow.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.85f), new Keyframe(Mathf.Min(0.2f / life, 0.3f), 1), new Keyframe(1, 1)));
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Mat_Ground" + material + ".mat");
        // Drawn before the other effects, which play over it
        renderer.sortingFudge = 10 - order;
    }

    // Where the effect meets the ground: a ring of light spreading from under it
    private static void Ring(Transform parent, float delay, float radius, Color color)
    {
        ParticleSystem ps = Make(parent, "ContactRing", delay, 0.4f, 1);
        var main = ps.main;
        main.startSize = radius * 2.4f;
        main.startColor = Color.white;
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        fade.color = Fade(Color.Lerp(color, Color.white, 0.35f), 1, 0.05f, 0.25f);
        var grow = ps.sizeOverLifetime;
        grow.enabled = true;
        grow.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.45f, 0, 2.5f), new Keyframe(1, 1, 0, 0)));
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Mat_GroundRing.mat");
        renderer.sortingFudge = 6;
    }

    // Dust thrown up from the ground's contact, rolling outwards and settling
    private static void Dust(Transform parent, float delay, float radius, int count)
    {
        ParticleSystem ps = Make(parent, "Dust", delay, 0, count);
        ps.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(radius * 0.9f, radius * 2f);
        main.startSize = new ParticleSystem.MinMaxCurve(radius * 0.35f, radius * 0.7f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.62f, 0.6f, 0.58f), new Color(0.78f, 0.77f, 0.76f));
        main.gravityModifier = -0.04f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius * 0.45f;
        shape.radiusThickness = 0.3f;
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.dampen = 0.2f;
        limit.limit = radius * 0.5f;
        var up = ps.velocityOverLifetime;
        up.enabled = true;
        up.space = ParticleSystemSimulationSpace.World;
        up.x = new ParticleSystem.MinMaxCurve(0, 0);
        up.y = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        up.z = new ParticleSystem.MinMaxCurve(0, 0);
        var grow = ps.sizeOverLifetime;
        grow.enabled = true;
        grow.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.6f), new Keyframe(1, 1.6f)));
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        fade.color = Fade(Color.white, 0.4f, 0.1f, 0.35f);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Mat_GroundStain.mat");
    }

    // A brief light on the floor at the impact: one invisible particle carrying a point light that fades with it
    private static void Flash(Transform parent, float delay, float radius, Color color, float intensity)
    {
        ParticleSystem ps = Make(parent, "Flash", delay, 0.35f, 1);
        ps.transform.localPosition = new Vector3(0, 0.7f, 0);
        var main = ps.main;
        main.startSize = 1;
        main.startColor = Color.Lerp(color, Color.white, 0.25f);
        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        fade.color = Fade(Color.white, 1, 0.05f, 0.15f);
        var template = new GameObject("FlashLight");
        template.transform.SetParent(ps.transform, false);
        var point = template.AddComponent<Light>();
        point.type = LightType.Point;
        point.range = 1;
        point.intensity = 1;
        point.shadows = LightShadows.None;
        template.SetActive(false);
        var lights = ps.lights;
        lights.enabled = true;
        lights.light = point;
        lights.ratio = 1;
        lights.maxLights = 1;
        lights.useParticleColor = true;
        lights.alphaAffectsIntensity = true;
        lights.sizeAffectsRange = false;
        lights.intensityMultiplier = intensity;
        lights.rangeMultiplier = radius * 3.5f;
        ps.GetComponent<ParticleSystemRenderer>().enabled = false;
    }
}
