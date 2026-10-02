using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
    [BoxGroup("UI"), Tooltip("The filter animating the health bars' health and armor (Assets/UI/Filters/HealthBar.asset)")]
    public FilterFunctionDefinition healthBarEffect;
    [BoxGroup("UI"), Tooltip("The UI's sounds, for the scene transition overlay created by code (Assets/Data/Audio/UISounds.asset)")]
    public UISounds uiSounds;

    [BoxGroup("Editions"), Required, Tooltip("The settings of the Anniversary (Assets/Data/Editions)")]
    public EditionProfile anniversaryProfile;
    [BoxGroup("Editions"), Required, Tooltip("The settings of the Classic, the original release's look")]
    public EditionProfile classicProfile;
    [BoxGroup("Editions"), Required, Tooltip("The Anniversary assets and their Classic counterparts")]
    public EditionSkin classicSkin;
    [BoxGroup("Editions"), Tooltip("The Classic's UI sheets (UI/Styles/Classic*.uss), addressable so that only the Classic loads them and their textures (ClassicStyles)")]
    public List<AssetReferenceT<StyleSheet>> classicSheets = new();

    public EditionProfile EditionProfile(GameEdition edition) => edition == GameEdition.Classic ? classicProfile : anniversaryProfile;
}
