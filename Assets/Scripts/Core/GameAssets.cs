using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

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

    [BoxGroup("UI"), Tooltip("The backdrop blur of the slanted shapes (Assets/UI/Filters/SlantedBlur.asset)")]
    public FilterFunctionDefinition slantedBlur;
    [BoxGroup("UI"), Tooltip("The panel of the scene transition overlay, created by code (Assets/UI/PanelSettings.asset)")]
    public PanelSettings panelSettings;
    [BoxGroup("UI"), Tooltip("The filter animating the inventory pieces by their rarity (Assets/UI/Filters/ArtifactPiece.asset)")]
    public FilterFunctionDefinition artifactPieceEffect;
}
