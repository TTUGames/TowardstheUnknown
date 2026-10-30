using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Fills the bands around the game's area (<see cref="Letterbox"/>) on a screen wider than 16:9 or taller than 16:10 (an
/// ultra-wide one, 4:3) with flat black, in both editions: left empty, they showed what the screen held before (the editor's
/// grey, stale frames). A document of its own, made at the first scene and kept: its bands lie outside its fitted area, where
/// no other document draws
/// </summary>
public class LetterboxBands : MonoBehaviour
{
    // Under every other document
    private const int SortingOrder = -100;

    private UIDocument document;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        // Inactive until configured: the UIDocument joins its panel in OnEnable
        var go = new GameObject(nameof(LetterboxBands));
        go.SetActive(false);
        DontDestroyOnLoad(go);
        var bands = go.AddComponent<LetterboxBands>();
        bands.document = go.AddComponent<UIDocument>();
        bands.document.panelSettings = GameAssets.Instance.panelSettings;
        bands.document.sortingOrder = SortingOrder;
        go.SetActive(true);
    }

    private void Start()
    {
        VisualElement root = document.rootVisualElement;
        root.pickingMode = PickingMode.Ignore;
        foreach (string side in new[] { "left", "right", "top", "bottom" })
        {
            var band = new VisualElement { pickingMode = PickingMode.Ignore };
            band.AddToClassList("letterbox-band");
            band.AddToClassList("letterbox-band--" + side);
            root.Add(band);
        }
        Letterbox.Fit(root);
    }
}
