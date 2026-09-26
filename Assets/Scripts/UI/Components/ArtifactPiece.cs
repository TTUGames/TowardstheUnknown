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
/// size. Turned in the hand, it swings to its new orientation instead of snapping. The piece itself is placed and turned
/// around its corner by the grid: the shrinking happens on an inner body, scaled around its center, and the swing on a
/// spin inside it, turned around the pointer
/// </summary>
public class ArtifactPiece : VisualElement
{
    private const float LineWidth = 2;
    private const long UpdateInterval = 33;
    // The swing of a quarter turn, in milliseconds, easing out without overshooting
    private const int TurnDuration = 140;
    private const string HoveredClass = "inventory-item--hovered";
    private const string HeldClass = "artifact-piece--held";
    private const string LandingClass = "artifact-piece--landing";

    private static readonly Color Line = new(1, 1, 1, 0.5f);
    private static readonly Color HoveredLine = new Color32(0xFF, 0xC9, 0xD7, 0xFF);

    private readonly List<List<Vector2>> outline;
    private readonly List<List<Vector2>> line;
    private readonly Color surface;
    private readonly VisualElement body = new() { pickingMode = PickingMode.Ignore };
    private readonly VisualElement spin = new() { pickingMode = PickingMode.Ignore };
    private readonly FilterFunctionDefinition effect;
    private readonly Color glow;
    private readonly float rarityAndSeed;
    private readonly float aspect;
    private bool hovered;
    // The spin's angle, in degrees clockwise: what is left of the swing, back to 0
    private float spinAngle;
    private ValueAnimation<float> turn;

    public ArtifactPiece(Artifact artifact, float cellSize)
    {
        pickingMode = PickingMode.Ignore;
        AddToClassList("artifact-piece");

        var cells = new HashSet<Vector2Int>(artifact.Slots);
        Vector2Int size = ArtifactPieceLayout.Size(cells);
        RarityPalette palette = GameAssets.Instance.rarityPalette;
        surface = palette != null ? palette.Get(artifact.Rarity, RarityPalette.Tone.Surface) : Color.gray;
        outline = ArtifactPieceLayout.Outline(cells, size, cellSize, ArtifactPieceLayout.Gap);
        // The line's middle runs half its width inside the outline: the whole line stays inside the shape
        line = ArtifactPieceLayout.Outline(cells, size, cellSize, ArtifactPieceLayout.Gap + LineWidth / 2);
        body.AddToClassList("artifact-piece__body");
        spin.AddToClassList("artifact-piece__spin");
        spin.generateVisualContent += Draw;
        body.Add(spin);
        Add(body);

        Sprite icon = artifact.SkillBarIcon;
        ArtifactPieceLayout.IconPlacement placement = icon != null
            ? ArtifactPieceLayout.PlaceIcon(cells, size, cellSize, icon, artifact.InventoryIconBounds, artifact.InventoryIconFit,
                artifact.InventoryIconScale, artifact.InventoryIconRotation, artifact.InventoryIconOffset)
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

        effect = GameAssets.Instance.artifactPieceEffect;
        if (palette != null) glow = palette.Get(artifact.Rarity, RarityPalette.Tone.Glow);
        // The seed keeps the pieces from sweeping together
        rarityAndSeed = (int)artifact.Rarity + 10 * Random.Range(0, 1000);
        aspect = (float)size.x / size.y;
        schedule.Execute(Tick).Every(UpdateInterval);
    }

    /// <summary>
    /// Plays the piece being taken in the hand: it shrinks a little, as if pressed. Next frame, so that the shrinking is
    /// animated from the full size
    /// </summary>
    public void Hold() => schedule.Execute(() => AddToClassList(HeldClass));

    /// <summary>
    /// Plays a quarter turn counterclockwise, the piece having just been turned to its new orientation: it swings there
    /// from the one it had, around the pointer (in panel coordinates). Turned again before it arrives, it swings on from
    /// where it is, so that quick turns add up
    /// </summary>
    public void PlayTurn(Vector2 pointer)
    {
        turn?.Stop();
        // The spin's pivot, where the pointer is on the piece: the rest of the piece swings around it
        Vector2 pivot = spin.WorldToLocal(pointer);
        spin.style.transformOrigin = new TransformOrigin(pivot.x, pivot.y);
        float from = spinAngle + 90;
        // At once, so that no frame shows the new orientation before the swing
        spinAngle = from;
        spin.style.rotate = new Rotate(from);
        turn = spin.experimental.animation.Start(from, 0, TurnDuration, (element, angle) =>
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

    private void Draw(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;
        painter.fillColor = surface;
        Trace(painter, outline);
        painter.Fill(FillRule.OddEven);

        painter.strokeColor = hovered ? HoveredLine : Line;
        painter.lineWidth = LineWidth;
        painter.lineJoin = LineJoin.Miter;
        Trace(painter, line);
        painter.Stroke();
    }

    private static void Trace(Painter2D painter, List<List<Vector2>> loops)
    {
        painter.BeginPath();
        foreach (List<Vector2> loop in loops)
        {
            painter.MoveTo(loop[0]);
            for (int i = 1; i < loop.Count; i++) painter.LineTo(loop[i]);
            painter.ClosePath();
        }
    }

    /// <summary>
    /// Follows the hover (the line's color) and advances the rarity's animation: a new filter with the time, which
    /// redraws the piece. Paused while detached
    /// </summary>
    private void Tick()
    {
        bool nowHovered = ClassListContains(HoveredClass);
        if (nowHovered != hovered)
        {
            hovered = nowHovered;
            spin.MarkDirtyRepaint();
        }
        if (effect == null) return;
        var function = new FilterFunction(effect);
        function.AddParameter(new FilterParameter(glow));
        function.AddParameter(new FilterParameter(rarityAndSeed));
        function.AddParameter(new FilterParameter(aspect));
        function.AddParameter(new FilterParameter(Time.unscaledTime % 1000));
        style.filter = new StyleList<FilterFunction>(new List<FilterFunction> { function });
    }
}
