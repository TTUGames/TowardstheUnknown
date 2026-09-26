using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The geometry of an artifact's piece in the inventory, shared by the piece (ArtifactPiece) and its preview in the
/// artifact's inspector: its outline, inset so that two pieces side by side keep a gap, the cells that cut the icon, and
/// where the icon goes. In points from the piece's top left corner, the shape's y axis going up
/// </summary>
public static class ArtifactPieceLayout
{
    // In points
    public const float Gap = 2;
    public const float Overlap = 1;

    /// <summary>
    /// The icon's whole sprite, turned by its rotation (degrees clockwise) around its pivot (in points from the sprite's top
    /// left corner), so that its drawing lands on its place
    /// </summary>
    public struct IconPlacement
    {
        public Rect rect;
        public Vector2 pivot;
        public float rotation;
    }

    public static Vector2Int Size(ICollection<Vector2Int> cells) => new(cells.Max(c => c.x) + 1, cells.Max(c => c.y) + 1);

    /// <summary>
    /// The cell's rect: inset by the gap on its outer sides, overlapping its neighbors by a pixel so that no seam shows
    /// </summary>
    public static Rect CellRect(HashSet<Vector2Int> cells, Vector2Int size, Vector2Int cell, float cellSize)
    {
        bool left = cells.Contains(cell + Vector2Int.left), right = cells.Contains(cell + Vector2Int.right);
        bool above = cells.Contains(cell + Vector2Int.up), below = cells.Contains(cell + Vector2Int.down);
        float x0 = cell.x * cellSize + (left ? 0 : Gap);
        float y0 = (size.y - 1 - cell.y) * cellSize + (above ? 0 : Gap);
        float x1 = (cell.x + 1) * cellSize + (right ? Overlap : -Gap);
        float y1 = (size.y - cell.y) * cellSize + (below ? Overlap : -Gap);
        return Rect.MinMaxRect(x0, y0, x1, y1);
    }

    /// <summary>
    /// The outline of the shape, as closed loops going clockwise on screen, moved inwards by the inset: the corners going
    /// in and out stay exact
    /// </summary>
    public static List<List<Vector2>> Outline(HashSet<Vector2Int> cells, Vector2Int size, float cellSize, float inset)
    {
        // The unit edges between a cell and the outside, clockwise on screen (y down), in cells
        var edges = new Dictionary<Vector2Int, List<Vector2Int>>();
        void AddEdge(Vector2Int from, Vector2Int to)
        {
            if (!edges.TryGetValue(from, out List<Vector2Int> list)) edges[from] = list = new List<Vector2Int>();
            list.Add(to);
        }
        foreach (Vector2Int cell in cells)
        {
            int x = cell.x, y = size.y - 1 - cell.y;
            if (!cells.Contains(cell + Vector2Int.up)) AddEdge(new(x, y), new(x + 1, y));
            if (!cells.Contains(cell + Vector2Int.right)) AddEdge(new(x + 1, y), new(x + 1, y + 1));
            if (!cells.Contains(cell + Vector2Int.down)) AddEdge(new(x + 1, y + 1), new(x, y + 1));
            if (!cells.Contains(cell + Vector2Int.left)) AddEdge(new(x, y + 1), new(x, y));
        }

        var loops = new List<List<Vector2>>();
        while (edges.Count > 0)
        {
            Vector2Int start = edges.Keys.First();
            var corners = new List<Vector2Int>();
            Vector2Int current = start;
            Vector2Int direction = Vector2Int.zero;
            do
            {
                List<Vector2Int> outgoing = edges[current];
                // Where two cells touch by a corner, keep turning right, so that each loop stays simple
                Vector2Int next = outgoing.Count == 1 || direction == Vector2Int.zero
                    ? outgoing[0]
                    : outgoing.OrderBy(to => TurnRank(direction, to - current)).First();
                outgoing.Remove(next);
                if (outgoing.Count == 0) edges.Remove(current);
                Vector2Int step = next - current;
                if (step != direction) corners.Add(current);
                direction = step;
                current = next;
            } while (current != start);
            // The start was only a corner if the loop turns there
            Vector2Int first = corners.Count > 1 ? corners[1] - corners[0] : Vector2Int.zero;
            if (corners.Count > 0 && Sign(first) == Sign(start - corners[^1])) corners.RemoveAt(0);

            var loop = new List<Vector2>();
            for (int i = 0; i < corners.Count; i++)
            {
                Vector2Int previous = corners[(i + corners.Count - 1) % corners.Count], corner = corners[i], following = corners[(i + 1) % corners.Count];
                Vector2 into = Sign(corner - previous), outOf = Sign(following - corner);
                // The inside is on the right of the way on screen: the corner moves along both inner normals
                Vector2 shift = (new Vector2(-into.y, into.x) + new Vector2(-outOf.y, outOf.x)) * inset;
                loop.Add((Vector2)corner * cellSize + shift);
            }
            loops.Add(loop);
        }
        return loops;
    }

    private static Vector2 Sign(Vector2Int v) => new(Mathf.Sign(v.x) * (v.x != 0 ? 1 : 0), Mathf.Sign(v.y) * (v.y != 0 ? 1 : 0));

    // 0 for a right turn on screen, then straight, then left
    private static int TurnRank(Vector2Int direction, Vector2Int step)
    {
        Vector2Int right = new(-direction.y, direction.x);
        return step == right ? 0 : step == direction ? 1 : 2;
    }

    /// <summary>
    /// Where the icon goes on the shape's largest rectangle: whole or covering it, never stretched, turned and moved as the
    /// artifact says
    /// </summary>
    public static IconPlacement PlaceIcon(HashSet<Vector2Int> cells, Vector2Int size, float cellSize, Sprite icon, Rect bounds,
        ArtifactIconFit fit, float scale, int rotation, Vector2 offset)
    {
        Rect cellsArea = LargestRectangle(cells, size);
        var area = new Rect(cellsArea.x * cellSize, (size.y - cellsArea.yMax) * cellSize, cellsArea.width * cellSize, cellsArea.height * cellSize);

        float drawingAspect = bounds.width * icon.rect.width / (bounds.height * icon.rect.height);
        rotation = ((rotation % 360) + 360) % 360;
        bool sideways = rotation == 90 || rotation == 270;

        // The drawing's box as it shows once turned, in its own proportions: whole in the rectangle, or covering it
        float shownAspect = sideways ? 1 / drawingAspect : drawingAspect;
        float width = fit == ArtifactIconFit.Fill
            ? Mathf.Max(area.width, area.height * shownAspect) * scale
            : Mathf.Min(area.width, area.height * shownAspect) * scale;
        var shown = new Vector2(width, width / shownAspect);
        // The drawing's box before turning, around the same center
        Vector2 drawing = sideways ? new Vector2(shown.y, shown.x) : shown;
        Vector2 center = area.center + new Vector2(offset.x, -offset.y) * cellSize;

        // The whole sprite, so that its drawing fills that box: the bounds go up from the bottom, the points down from the top
        var sprite = new Vector2(drawing.x / bounds.width, drawing.y / bounds.height);
        var topLeft = new Vector2(center.x - drawing.x / 2 - bounds.x * sprite.x, center.y - drawing.y / 2 - (1 - bounds.yMax) * sprite.y);
        return new IconPlacement { rect = new Rect(topLeft, sprite), pivot = center - topLeft, rotation = rotation };
    }

    /// <summary>
    /// The largest rectangle made only of the shape's cells, in cells from its bottom left corner; among equals the
    /// squarest, then the closest to the shape's center
    /// </summary>
    public static Rect LargestRectangle(HashSet<Vector2Int> cells, Vector2Int size)
    {
        Rect best = new Rect(cells.First().x, cells.First().y, 1, 1);
        float bestScore = float.MinValue;
        Vector2 center = (Vector2)size / 2;
        for (int x0 = 0; x0 < size.x; x0++)
        for (int y0 = 0; y0 < size.y; y0++)
        for (int x1 = x0; x1 < size.x; x1++)
        for (int y1 = y0; y1 < size.y; y1++)
        {
            bool full = true;
            for (int x = x0; x <= x1 && full; x++)
                for (int y = y0; y <= y1 && full; y++)
                    full = cells.Contains(new Vector2Int(x, y));
            if (!full) continue;
            var rect = new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
            float score = rect.width * rect.height * 100 + Mathf.Min(rect.width, rect.height) * 10 - Vector2.Distance(rect.center, center);
            if (score > bestScore) { bestScore = score; best = rect; }
        }
        return best;
    }
}
