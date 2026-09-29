using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The ribbon a blade leaves behind during the swing of an attack: each frame of the swing keeps the blade's segment,
/// from its hilt to its tip, for <c>lifetime</c> seconds, smoothed between frames, drawn by an additive material in the
/// attack's color. <see cref="AttackAnimationAction"/> asks for it over the swing (<see cref="Swing"/>); in scaled time,
/// so that a hit stop holds it. Drawn only while the blade shows
/// </summary>
public class WeaponTrail : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [SerializeField, Required, Tooltip("The blade's object, whose segment the trail follows")] private GameObject blade;
    [SerializeField, Tooltip("The segment's ends in the blade's local space: the guard and the tip")] private Vector3 hilt = new Vector3(0, 1.2f, 0);
    [SerializeField] private Vector3 tip = new Vector3(0, 4, 0);
    [SerializeField, Required] private Material material;
    [SerializeField, Min(0.02f), SuffixLabel("s"), Tooltip("How long a slice of the trail lasts")] private float lifetime = 0.13f;
    [SerializeField, Range(1, 8), Tooltip("Slices added between two frames' segments, along a curve through them")] private int subdivisions = 4;
    [SerializeField, Min(0), Tooltip("Multiplies the attack's color (HDR)")] private float intensity = 3;
    [SerializeField, Tooltip("The color of an attack without one, or with a pale one")] private Color defaultColor = new Color(0.35f, 0.6f, 1);
    [SerializeField, Range(0, 1), Tooltip("Below this saturation, the attack's color counts as pale")] private float minSaturation = 0.2f;

    private struct Slice
    {
        public Vector3 hilt, tip;
        public float time;
    }

    private readonly List<Slice> slices = new List<Slice>();
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector2> uvs = new List<Vector2>();
    private readonly List<int> triangles = new List<int>();
    private GameObject view;
    private MeshRenderer viewRenderer;
    private Mesh mesh;
    private MaterialPropertyBlock block;
    private Coroutine swing;
    private bool emitting;

    private void Awake()
    {
        mesh = new Mesh { name = "Weapon Trail" };
        mesh.MarkDynamic();
        view = new GameObject("Weapon Trail");
        view.transform.SetParent(transform, false);
        view.AddComponent<MeshFilter>().sharedMesh = mesh;
        viewRenderer = view.AddComponent<MeshRenderer>();
        viewRenderer.sharedMaterial = material;
        viewRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        viewRenderer.receiveShadows = false;
        block = new MaterialPropertyBlock();
    }

    private void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
    }

    private void OnDisable()
    {
        emitting = false;
        swing = null;
        slices.Clear();
        if (view != null) view.SetActive(false);
    }

    private void OnEnable()
    {
        if (view != null) view.SetActive(true);
    }

    /// <summary>
    /// Leaves a trail from <paramref name="start"/> to <paramref name="end"/> seconds from now, in the color (the default if clear)
    /// </summary>
    public void Swing(float start, float end, Color color)
    {
        if (!isActiveAndEnabled) return;
        if (swing != null) StopCoroutine(swing);
        swing = StartCoroutine(Emit(start, end));
        block.SetColor(ColorId, TrailColor(color) * intensity);
        viewRenderer.SetPropertyBlock(block);
    }

    /// <summary>
    /// The attack's hue at full value; a pale or missing one (white, gray) takes the default, which additive white would blow out
    /// </summary>
    private Color TrailColor(Color color)
    {
        Color.RGBToHSV(color, out float hue, out float saturation, out _);
        return color.a <= 0 || saturation < minSaturation ? defaultColor : Color.HSVToRGB(hue, saturation, 1);
    }

    private IEnumerator Emit(float start, float end)
    {
        if (start > 0) yield return new WaitForSeconds(start);
        emitting = true;
        yield return new WaitForSeconds(Mathf.Max(0, end - start));
        emitting = false;
        swing = null;
    }

    //After the animation and the IK: the blade where it is drawn
    private void LateUpdate()
    {
        float now = Time.time;
        if (emitting && blade.activeInHierarchy)
        {
            Transform bladeTransform = blade.transform;
            Vector3 hiltPoint = bladeTransform.TransformPoint(hilt), tipPoint = bladeTransform.TransformPoint(tip);
            //A hit stop adds no slice: the time stands still
            if (slices.Count == 0 || now > slices[^1].time) slices.Add(new Slice { hilt = hiltPoint, tip = tipPoint, time = now });
        }
        while (slices.Count > 0 && now - slices[0].time > lifetime) slices.RemoveAt(0);
        Build(now);
    }

    private void Build(float now)
    {
        mesh.Clear();
        if (slices.Count < 2) return;
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        Transform space = view.transform;
        //From the newest slice (the blade) to the oldest
        for (int i = slices.Count - 1; i > 0; i--)
        {
            Slice a = slices[i], b = slices[i - 1];
            Slice before = i + 1 < slices.Count ? slices[i + 1] : a, after = i - 2 >= 0 ? slices[i - 2] : b;
            int steps = i == 1 ? subdivisions + 1 : subdivisions;
            for (int s = 0; s < steps; s++)
            {
                float t = (float)s / subdivisions;
                Vector3 h = CatmullRom(before.hilt, a.hilt, b.hilt, after.hilt, t);
                Vector3 p = CatmullRom(before.tip, a.tip, b.tip, after.tip, t);
                float age = Mathf.Clamp01((now - Mathf.Lerp(a.time, b.time, t)) / lifetime);
                vertices.Add(space.InverseTransformPoint(h));
                vertices.Add(space.InverseTransformPoint(p));
                uvs.Add(new Vector2(age, 0));
                uvs.Add(new Vector2(age, 1));
            }
        }
        for (int v = 0; v + 3 < vertices.Count; v += 2)
        {
            triangles.Add(v); triangles.Add(v + 1); triangles.Add(v + 2);
            triangles.Add(v + 1); triangles.Add(v + 3); triangles.Add(v + 2);
        }
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t, t3 = t2 * t;
        return 0.5f * (2 * p1 + (p2 - p0) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (3 * p1 - p0 - 3 * p2 + p3) * t3);
    }
}
