using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The assets of the Anniversary and their Classic counterparts, restored from the original release
/// (Assets/Data/Editions/ClassicSkin.asset): the only place that knows both editions' assets.
/// <see cref="EditionMaterials"/> swaps the materials, <see cref="Resolve(GameObject)"/> the prefabs instantiated by code
/// </summary>
[CreateAssetMenu(fileName = "ClassicSkin", menuName = "TTU/Edition Skin")]
public class EditionSkin : ScriptableObject
{
    [Serializable]
    public struct MaterialPair
    {
        [AssetsOnly] public Material anniversary;
        [AssetsOnly] public Material classic;
    }

    [Serializable]
    public struct PrefabPair
    {
        [AssetsOnly] public GameObject anniversary;
        [AssetsOnly] public GameObject classic;
    }

    [SerializeField, TableList, Tooltip("The shared materials: decor, tiles, plants, water, and the VFX materials on a shader of the original")]
    private MaterialPair[] materials = Array.Empty<MaterialPair>();
    [SerializeField, TableList, Tooltip("The prefabs replaced as a whole when code instantiates them")]
    private PrefabPair[] prefabs = Array.Empty<PrefabPair>();

    private Dictionary<Material, Material> toClassic;
    private Dictionary<GameObject, GameObject> classicPrefabs;

    public bool HasMaterials => materials.Length > 0;

    // Built again after an edit, and in each Play mode (no domain reload)
    private void OnEnable() => ClearCache();

    private void OnValidate() => ClearCache();

    private void ClearCache()
    {
        toClassic = null;
        classicPrefabs = null;
    }

    private void BuildCache()
    {
        toClassic = new Dictionary<Material, Material>();
        foreach (MaterialPair pair in materials)
            if (pair.anniversary != null && pair.classic != null)
                toClassic[pair.anniversary] = pair.classic;
        classicPrefabs = new Dictionary<GameObject, GameObject>();
        foreach (PrefabPair pair in prefabs)
            if (pair.anniversary != null && pair.classic != null)
                classicPrefabs[pair.anniversary] = pair.classic;
    }

    /// <summary>
    /// The Classic material of an Anniversary one; itself if it has no pair. Several Anniversary materials may share a Classic
    /// one: the way back is kept per renderer by <see cref="EditionMaterials"/>
    /// </summary>
    public Material Classic(Material material)
    {
        if (material == null) return null;
        if (toClassic == null) BuildCache();
        return toClassic.TryGetValue(material, out Material classic) ? classic : material;
    }

    /// <summary>
    /// The prefab to instantiate in the current edition for an Anniversary prefab
    /// </summary>
    public GameObject Resolve(GameObject prefab)
    {
        if (prefab == null || !Edition.IsClassic) return prefab;
        if (classicPrefabs == null) BuildCache();
        return classicPrefabs.TryGetValue(prefab, out GameObject classic) ? classic : prefab;
    }
}
