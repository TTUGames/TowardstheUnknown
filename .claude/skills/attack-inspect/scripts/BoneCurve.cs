using System.Linq; using UnityEditor; using UnityEngine;
public static class BoneCurve {
  // Samples a clip on a prefab's model and prints a bone's height (and its speed) over time
  public static string Run(string prefab, string clipPath, string clipName, string bone, float step) {
    var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefab));
    try {
      var clip = AssetDatabase.LoadAllAssetsAtPath(clipPath).OfType<AnimationClip>().First(c => c.name == clipName);
      var anim = go.GetComponentInChildren<Animator>(); var root = anim.gameObject;
      var b = go.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name.ToLower().Contains(bone.ToLower()));
      if (b == null) return "no bone " + bone + ": " + string.Join(",", go.GetComponentsInChildren<Transform>().Select(t => t.name).Take(80));
      var sb = new System.Text.StringBuilder($"{clipName} {clip.length:0.00}s bone {b.name}\n"); Vector3 prev = Vector3.zero;
      for (float t = 0; t <= clip.length + 1e-4f; t += step) { clip.SampleAnimation(root, t); var p = b.position - go.transform.position; float v = t > 0 ? (p - prev).magnitude / step : 0; prev = p;
        sb.Append($"{t:0.00} y={p.y:0.00} fwd={Vector3.Dot(p, go.transform.forward):0.00} v={v:0.0}\n"); }
      return sb.ToString();
    } finally { Object.DestroyImmediate(go); } } }
