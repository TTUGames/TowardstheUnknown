using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// The assets of the Anniversary and their Classic counterparts, restored from the original release
/// (Assets/Data/Editions/ClassicSkin.asset): the only place that knows both editions' assets.
/// <see cref="EditionMaterials"/> swaps the materials, <see cref="Resolve(GameObject)"/> the prefabs instantiated by code,
/// <see cref="EntityAnimator"/> the animation clips (<see cref="Current(AnimationClip)"/>).
/// The Anniversary sides are referenced directly (the scenes load them anyway); the Classic materials and prefabs are
/// addressable (group ClassicSkin, <c>classicAsset</c>), loaded the first time the Classic is shown and kept then, so that a
/// game played in the Anniversary never loads them nor their textures. A Classic side the scenes load anyway (a material
/// embedded in a model they show) and the clips are referenced directly (<c>classic</c>): a bundle would copy their model
/// </summary>
[CreateAssetMenu(fileName = "ClassicSkin", menuName = "TTU/Edition Skin")]
public class EditionSkin : ScriptableObject
{
    [Serializable]
    public struct MaterialPair
    {
        [AssetsOnly] public Material anniversary;
        [AssetsOnly, Tooltip("A Classic side the scenes load anyway (a material embedded in a model they show); otherwise empty")] public Material classic;
        [Tooltip("The Classic side, addressable (group ClassicSkin): loaded with the Classic")] public AssetReferenceT<Material> classicAsset;
    }

    [Serializable]
    public struct PrefabPair
    {
        [AssetsOnly] public GameObject anniversary;
        [AssetsOnly, Tooltip("A Classic side the scenes load anyway; otherwise empty")] public GameObject classic;
        [Tooltip("The Classic side, addressable (group ClassicSkin): loaded with the Classic")] public AssetReferenceGameObject classicAsset;
    }

    [Serializable]
    public struct ClipPair
    {
        [AssetsOnly] public AnimationClip anniversary;
        [AssetsOnly, Tooltip("Empty: the Classic plays no clip there, as the original (an attack without animation)")] public AnimationClip classic;
    }

    [SerializeField, TableList, Tooltip("The shared materials: decor, tiles, plants, water, and the VFX materials on a shader of the original")]
    private MaterialPair[] materials = Array.Empty<MaterialPair>();
    [SerializeField, TableList, Tooltip("The prefabs replaced as a whole when code instantiates them")]
    private PrefabPair[] prefabs = Array.Empty<PrefabPair>();
    [SerializeField, TableList, Tooltip("The animation clips the Anniversary changed, paired with the original's: the entities' clips and the abilities' attacks")]
    private ClipPair[] clips = Array.Empty<ClipPair>();

    private Dictionary<Material, Material> toClassic;
    private Dictionary<GameObject, GameObject> classicPrefabs;
    private Dictionary<AnimationClip, AnimationClip> classicClips;
    // The Classic sides loaded, kept until the cache is cleared
    private readonly List<AsyncOperationHandle> handles = new();

    public bool HasMaterials => materials.Length > 0;

    // The skins whose cache was built: each Play mode starts without (no domain reload), the Classic sides unloaded
    private static readonly List<EditionSkin> built = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        foreach (EditionSkin skin in built)
            if (skin != null) skin.ClearCache();
        built.Clear();
    }

    // Built again after an edit
    private void OnEnable() => ClearCache();

    private void OnValidate() => ClearCache();

    private void ClearCache()
    {
        foreach (AsyncOperationHandle handle in handles)
            if (handle.IsValid()) handle.Release();
        handles.Clear();
        toClassic = null;
        classicPrefabs = null;
        classicClips = null;
    }

    private void BuildCache()
    {
        if (!built.Contains(this)) built.Add(this);
        // Built the first time the Classic needs a pair: the addressable sides load then, at once (a switch happens
        // behind its wipe, a game started in the Classic before its first scene)
        toClassic = new Dictionary<Material, Material>();
        foreach (MaterialPair pair in materials)
        {
            Material classic = pair.classic != null ? pair.classic : Load(pair.classicAsset);
            if (pair.anniversary != null && classic != null)
                toClassic[pair.anniversary] = classic;
        }
        classicPrefabs = new Dictionary<GameObject, GameObject>();
        foreach (PrefabPair pair in prefabs)
        {
            GameObject classic = pair.classic != null ? pair.classic : Load(pair.classicAsset);
            if (pair.anniversary != null && classic != null)
                classicPrefabs[pair.anniversary] = classic;
        }
        classicClips = new Dictionary<AnimationClip, AnimationClip>();
        //An empty Classic side is kept: the clip plays nothing in the Classic
        foreach (ClipPair pair in clips)
            if (pair.anniversary != null)
                classicClips[pair.anniversary] = pair.classic;
    }

    private T Load<T>(AssetReferenceT<T> reference) where T : UnityEngine.Object
    {
        if (reference == null || !reference.RuntimeKeyIsValid()) return null;
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(reference.RuntimeKey);
        handle.WaitForCompletion();
        handles.Add(handle);
        return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null;
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
    /// The material to give in the current edition for an Anniversary one, to code setting a renderer's material at runtime
    /// (the tile overlays), which <see cref="EditionMaterials"/> doesn't see
    /// </summary>
    public Material Current(Material material) => Edition.IsClassic ? Classic(material) : material;

    /// <summary>
    /// The prefab to instantiate in the current edition for an Anniversary prefab
    /// </summary>
    public GameObject Resolve(GameObject prefab)
    {
        if (prefab == null || !Edition.IsClassic) return prefab;
        if (classicPrefabs == null) BuildCache();
        return classicPrefabs.TryGetValue(prefab, out GameObject classic) ? classic : prefab;
    }

    /// <summary>
    /// The clip to play in the current edition for an Anniversary clip; itself if it has no pair, null if the Classic plays none there
    /// </summary>
    public AnimationClip Current(AnimationClip clip)
    {
        if (clip == null || !Edition.IsClassic) return clip;
        if (classicClips == null) BuildCache();
        return classicClips.TryGetValue(clip, out AnimationClip classic) ? classic : clip;
    }
}
