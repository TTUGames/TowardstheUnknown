using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Fills the bands around the 16:9 area (<see cref="Letterbox"/>) on a screen of another shape (a Steam Deck's 16:10, an
/// ultra-wide one) with a soft gradient, darker away from the game, in an edition with <see cref="EditionProfile.letterboxFill"/>;
/// the Classic keeps the original's flat black. A document of its own, made at the first scene and kept: its bands lie outside
/// its fitted area, where no other document draws
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
        root.Add(new Band("left", Vector2.left));
        root.Add(new Band("right", Vector2.right));
        root.Add(new Band("top", Vector2.down));
        root.Add(new Band("bottom", Vector2.up));
        Letterbox.Fit(root);
        Refresh(Edition.Current);
        Edition.Changed += Refresh;
    }

    private void OnDestroy() => Edition.Changed -= Refresh;

    private void Refresh(GameEdition edition) => document.rootVisualElement.EnableInClassList("hidden", !Edition.Profile.letterboxFill);

    /// <summary>
    /// A band, from the game's edge outward: its --band-inner-color fading to its --band-outer-color over --band-ramp points,
    /// in strips of plain color, then flat (a stretched texture or a drawn mesh this large came out partly black)
    /// </summary>
    private class Band : VisualElement
    {
        private static readonly CustomStyleProperty<Color> innerColorProperty = new("--band-inner-color");
        private static readonly CustomStyleProperty<Color> outerColorProperty = new("--band-outer-color");
        private static readonly CustomStyleProperty<float> rampProperty = new("--band-ramp");
        private const int RampSteps = 32;

        // Away from the game, in the panel's axes (y down)
        private readonly Vector2 outward;
        private readonly VisualElement[] strips = new VisualElement[RampSteps + 1];

        public Band(string side, Vector2 outward)
        {
            this.outward = outward;
            pickingMode = PickingMode.Ignore;
            AddToClassList("letterbox-band");
            AddToClassList("letterbox-band--" + side);
            for (int i = 0; i < strips.Length; i++)
            {
                strips[i] = new VisualElement { pickingMode = PickingMode.Ignore };
                strips[i].style.position = Position.Absolute;
                Add(strips[i]);
            }
            RegisterCallback<CustomStyleResolvedEvent>(_ => Layout());
        }

        // The strips' places and colors, the last one flat to the band's far end
        private void Layout()
        {
            customStyle.TryGetValue(innerColorProperty, out Color inner);
            customStyle.TryGetValue(outerColorProperty, out Color outer);
            float ramp = customStyle.TryGetValue(rampProperty, out float value) ? Mathf.Max(1, value) : 1;
            for (int i = 0; i < strips.Length; i++)
            {
                IStyle style = strips[i].style;
                float from = ramp * i / RampSteps;
                bool flat = i == RampSteps;
                float t = (i + 0.5f) / RampSteps;
                style.backgroundColor = flat ? outer : Color.Lerp(inner, outer, t * t * (3 - 2 * t));
                // Across the band whole; along it from the game's edge, overlapping the next strip by a point
                StyleLength start = from, size = flat ? new StyleLength(StyleKeyword.Auto) : ramp / RampSteps + 1;
                if (outward.x != 0)
                {
                    style.top = 0;
                    style.bottom = 0;
                    if (outward.x < 0) style.right = start; else style.left = start;
                    if (flat) { if (outward.x < 0) style.left = 0; else style.right = 0; }
                    else style.width = size;
                }
                else
                {
                    style.left = 0;
                    style.right = 0;
                    if (outward.y < 0) style.bottom = start; else style.top = start;
                    if (flat) { if (outward.y < 0) style.top = 0; else style.bottom = 0; }
                    else style.height = size;
                }
            }
        }
    }
}
