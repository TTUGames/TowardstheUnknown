using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// References the assets used by code that has no scene object to hold them.
/// The only asset loaded from Resources: keep it at Resources/GameAssets.
/// </summary>
[CreateAssetMenu(fileName = "GameAssets", menuName = "TTU/Game Assets")]
public class GameAssets : ScriptableObject
{
    private static GameAssets instance;

    public static GameAssets Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<GameAssets>("GameAssets");
            return instance;
        }
    }

    [BoxGroup("Prefabs")] public Collectable collectable;
    [BoxGroup("Prefabs")] public TileOverlay tileOverlay;
    [BoxGroup("Prefabs")] public DamageIndicator damageIndicator;

    [BoxGroup("VFX")] public GameObject hit;
    [BoxGroup("VFX"), Tooltip("Shown on the exits of a cleared room")] public GameObject roomExit;
    [BoxGroup("VFX")] public GameObject draregPhaseTransition;
    [BoxGroup("VFX")] public GameObject draregChains;
    [BoxGroup("VFX"), Tooltip("Aura of a collectable, indexed by its best artifact rarity: common, rare, epic, legendary")]
    public GameObject[] dropAuras = new GameObject[4];
}
