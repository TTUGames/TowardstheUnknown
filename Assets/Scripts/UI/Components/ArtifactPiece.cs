using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// An artifact's piece in the inventory, built from its shape, its rarity and its skill bar icon (ArtifactPieceLayout): its
/// outline, inset so that two pieces side by side keep a gap, drawn as one shape in the rarity's surface tone with a line
/// running only inside it, and the icon laid on the shape's largest rectangle (whole, or covering it and cut by the shape, never stretched).
/// Transparent cells cut the icon to the shape. Sized in cells of the unrotated shape, the y axis going up as in the grid.
/// A filter (UI/Filters/ArtifactPiece.shader) animates it by its rarity, like the relics on the floor: redrawn about 30
/// times a second while it is shown. Held in the hand, it shrinks a little, as if pressed; put down, it grows back to its
/// size, overshooting a little, or shakes in the accent color when it comes back from a place it didn't fit. Turned in
/// the hand, it swings to its new orientation instead of snapping. The piece itself is placed and turned
/// around its corner by the grid: the shrinking happens on an inner body, scaled around its center, and the swing on a
/// spin inside it, both around the point grabbed, which stays under the pointer
/// </summary>
public class ArtifactPiece : VisualElement
{
    private const float LineWidth = 2;
    private const long UpdateInterval = 33;
    private const string HeldClass = "artifact-piece--held";
    private const string LandingClass = "artifact-piece--landing";
    private const string RefusedClass = "artifact-piece--refused";
    private const string ConcealedClass = "artifact-piece--concealed";
    private const string RevealingClass = "artifact-piece--revealing";

    // The outline's color, set by Inventory.uss (tinted while the piece is hovered)
    private static readonly CustomStyleProperty<Color> lineColorProperty = new("--piece-line-color");
    // 0 (the Classic edition's style) leaves the rarity's animated filter out
    private static readonly CustomStyleProperty<float> effectsProperty = new("--ui-effects");
    // 0 leaves the drawn surface and outline out: the Classic shows the original's sprite of the piece instead
    private static readonly CustomStyleProperty<float> drawnProperty = new("--piece-drawn");
    // The swing of a quarter turn (UssTime), easing out without overshooting
    private static readonly CustomStyleProperty<string> turnDurationProperty = new("--turn-duration");
    // The rarity's flash of a piece coming into a chest, fading out (UssTime)
    private static readonly CustomStyleProperty<string> flashDurationProperty = new("--flash-duration");
    private bool effects = true;
    private bool drawn = true;

    private readonly List<List<Vector2>> outline;
    private readonly List<List<Vector2>> line;
    private readonly Color surface;
    private readonly VisualElement body = new() { pickingMode = PickingMode.Ignore };
    private readonly VisualElement spin = new() { pickingMode = PickingMode.Ignore };
    private readonly FilterFunctionDefinition effect;
    private readonly Color glow;
    private readonly Color accent;
    // The rarity's flash over the surface, from 1 to 0 (Reveal)
    private float flash;
    private ValueAnimation<float> flashAnimation;
    private readonly float rarityAndSeed;
    private readonly float aspect;
    private Color lineColor = Color.white;
    // The spin's angle, in degrees clockwise: what is left of the swing, back to 0
    private float spinAngle;
    private ValueAnimation<float> turn;

    /// <param name="palette">The rarities' colors: the piece is gray without it</param>
    public ArtifactPiece(Artifact artifact, float cellSize, RarityPalette palette)
    {
        pickingMode = PickingMode.Ignore;
        AddToClassList("artifact-piece");
        RegisterCallback<CustomStyleResolvedEvent>(_ => ReadStyle());

        var cells = new HashSet<Vector2Int>(artifact.Data.shape);
        Vector2Int size = ArtifactPieceLayout.Size(cells);
        surface = palette != null ? palette.Get(artifact.Data.rarity, RarityPalette.Tone.Surface) : Color.gray;
        outline = ArtifactPieceLayout.Outline(cells, size, cellSize, ArtifactPieceLayout.Gap);
        // The line's middle runs half its width inside the outline: the whole line stays inside the shape
        line = ArtifactPieceLayout.Outline(cells, size, cellSize, ArtifactPieceLayout.Gap + LineWidth / 2);
        body.AddToClassList("artifact-piece__body");
        body.AddToClassList("stretch");
        spin.AddToClassList("artifact-piece__spin");
        spin.AddToClassList("stretch");
        spin.generateVisualContent += Draw;
        body.Add(spin);
        Add(body);

        Sprite icon = artifact.Data.skillBarIcon;
        ArtifactPieceLayout.IconPlacement placement = icon != null
            ? ArtifactPieceLayout.PlaceIcon(cells, size, cellSize, icon, artifact.Data.inventoryIconBounds, artifact.Data.inventoryIconFit,
                artifact.Data.inventoryIconScale, artifact.Data.inventoryIconRotation, artifact.Data.inventoryIconOffset)
            : default;
        foreach (Vector2Int cell in cells)
        {
            Rect rect = ArtifactPieceLayout.CellRect(cells, size, cell, cellSize);
            var element = new VisualElement { pickingMode = PickingMode.Ignore };
            element.AddToClassList("artifact-piece__cell");
            element.style.left = rect.x;
            element.style.top = rect.y;
            element.style.width = rect.width;
            element.style.height = rect.height;
            if (icon != null)
            {
                // The part of the icon over this cell
                var part = new VisualElement { pickingMode = PickingMode.Ignore };
                part.AddToClassList("artifact-piece__icon");
                part.style.backgroundImage = new StyleBackground(icon);
                part.style.left = placement.rect.x - rect.x;
                part.style.top = placement.rect.y - rect.y;
                part.style.width = placement.rect.width;
                part.style.height = placement.rect.height;
                part.style.transformOrigin = new TransformOrigin(placement.pivot.x, placement.pivot.y);
                part.style.rotate = new Rotate(placement.rotation);
                element.Add(part);
            }
            spin.Add(element);
        }
        // The original release's piece, whole: hidden unless a style shows it (ClassicInventory.uss)
        if (artifact.Data.classicInventorySprite != null && artifact.Data.classicInventorySprite.RuntimeKeyIsValid())
        {
            var original = new VisualElement { pickingMode = PickingMode.Ignore };
            original.AddToClassList("artifact-piece__original");
            original.AddToClassList("stretch");
            //Loaded in the Classic only
            ClassicStyles.Image(original, artifact.Data.classicInventorySprite);
            spin.Add(original);
            AddToClassList("artifact-piece--has-original");
        }

        effect = GameAssets.Instance.artifactPieceEffect;
        if (palette != null)
        {
            glow = palette.Get(artifact.Data.rarity, RarityPalette.Tone.Glow);
            accent = palette.Get(artifact.Data.rarity, RarityPalette.Tone.Accent);
        }
        // The seed keeps the pieces from sweeping together
        rarityAndSeed = (int)artifact.Data.rarity + 10 * Random.Range(0, 1000);
        aspect = (float)size.x / size.y;
        schedule.Execute(Tick).Every(UpdateInterval);
    }

    /// <summary>
    /// Plays the piece being taken in the hand: it shrinks a little, as if pressed, around its anchor, the point grabbed
    /// (in the piece's unrotated coordinates, y down), which stays under the pointer. It swings around the same point.
    /// Next frame, so that the shrinking is animated from the full size
    /// </summary>
    public void Hold(Vector2 anchor)
    {
        var origin = new TransformOrigin(anchor.x, anchor.y);
        body.style.transformOrigin = origin;
        spin.style.transformOrigin = origin;
        schedule.Execute(() => AddToClassList(HeldClass));
    }

    /// <summary>
    /// Plays a quarter turn counterclockwise, the piece having just been turned to its new orientation around its anchor:
    /// it swings there from the one it had. Turned again before it arrives, it swings on from where it is, so that quick
    /// turns add up
    /// </summary>
    public void PlayTurn()
    {
        turn?.Stop();
        float from = spinAngle + 90;
        // At once, so that no frame shows the new orientation before the swing
        spinAngle = from;
        spin.style.rotate = new Rotate(from);
        turn = spin.experimental.animation.Start(from, 0, (int)customStyle.Milliseconds(turnDurationProperty, 0), (element, angle) =>
        {
            spinAngle = angle;
            element.style.rotate = new Rotate(angle);
        }).Ease(Easing.OutCubic);
        // Stopped or done, the animation is recycled: forget it
        turn.OnCompleted(() => turn = null);
    }

    /// <summary>
    /// Plays the piece being put down: from the held size, it grows back to its own
    /// </summary>
    public void Land()
    {
        AddToClassList(LandingClass);
        schedule.Execute(() => RemoveFromClassList(LandingClass)).StartingIn(16);
    }

    /// <summary>
    /// Plays the piece coming back from a place it didn't fit: it lands and shakes, its outline in the accent color
    /// </summary>
    public void LandRefused()
    {
        Land();
        // The body shakes: the grid places the piece itself
        RefuseShake.Play(body, RefusedClass, this);
    }

    /// <summary>
    /// Hides the piece until <see cref="Reveal"/>
    /// </summary>
    public void Conceal() => AddToClassList(ConcealedClass);

    /// <summary>
    /// Shows the piece hidden by <see cref="Conceal"/> at once, without its reveal
    /// </summary>
    public void Unconceal() => RemoveFromClassList(ConcealedClass);

    /// <summary>
    /// Plays the piece coming into a chest: it grows from small, overshooting a little, its surface flashing in its rarity's color
    /// </summary>
    public void Reveal()
    {
        RemoveFromClassList(ConcealedClass);
        AddToClassList(RevealingClass);
        schedule.Execute(() => RemoveFromClassList(RevealingClass)).StartingIn(16);
        flashAnimation?.Stop();
        flashAnimation = spin.experimental.animation.Start(1, 0, (int)customStyle.Milliseconds(flashDurationProperty, 0), (element, value) =>
        {
            flash = value;
            element.MarkDirtyRepaint();
        }).Ease(Easing.OutQuad);
        flashAnimation.OnCompleted(() => flashAnimation = null);
    }

    private void Draw(MeshGenerationContext context)
    {
        if (!drawn) return;
        Painter2D painter = context.painter2D;
        painter.fillColor = surface;
        Trace(painter, outline);
        painter.Fill(FillRule.OddEven);

        painter.strokeColor = lineColor;
        painter.lineWidth = LineWidth;
        painter.lineJoin = LineJoin.Miter;
        Trace(painter, line);
        painter.Stroke();

        if (flash <= 0) return;
        painter.fillColor = new Color(accent.r, accent.g, accent.b, flash);
        Trace(painter, outline);
        painter.Fill(FillRule.OddEven);
    }

    private static void Trace(Painter2D painter, List<List<Vector2>> loops)
    {
        painter.BeginPath();
        foreach (List<Vector2> loop in loops) painter.TracePolygon(loop);
    }

    /// <summary>
    /// Reads the outline's color from USS, which changes it when the piece is hovered, and redraws the outline
    /// </summary>
    private void ReadStyle()
    {
        effects = !customStyle.TryGetValue(effectsProperty, out float effectsValue) || effectsValue > 0;
        bool draw = !customStyle.TryGetValue(drawnProperty, out float drawnValue) || drawnValue > 0;
        if (draw != drawn)
        {
            drawn = draw;
            spin.MarkDirtyRepaint();
        }
        if (!customStyle.TryGetValue(lineColorProperty, out Color color) || color == lineColor) return;
        lineColor = color;
        spin.MarkDirtyRepaint();
    }

    /// <summary>
    /// Advances the rarity's animation: a new filter with the time, which redraws the piece. Paused while detached or
    /// hidden (the inventory closed): the time picks it up where it would have been
    /// </summary>
    private void Tick()
    {
        if (!effects)
        {
            if (style.filter.keyword != StyleKeyword.Null) style.filter = StyleKeyword.Null;
            return;
        }
        if (effect == null || !this.IsShown()) return;
        var function = new FilterFunction(effect);
        function.AddParameter(new FilterParameter(glow));
        function.AddParameter(new FilterParameter(rarityAndSeed));
        function.AddParameter(new FilterParameter(aspect));
        function.AddParameter(new FilterParameter(Time.unscaledTime % 1000));
        style.filter = new StyleList<FilterFunction>(new List<FilterFunction> { function });
    }
}
