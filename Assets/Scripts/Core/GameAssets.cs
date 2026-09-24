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

    [BoxGroup("Prefabs")] public TileOverlay tileOverlay;

    [BoxGroup("VFX")] public GameObject hit;
}
