using System;

/// <summary>
/// Draws, under the field, the artifact's piece as the inventory shows it (ArtifactPieceLayout): its shape in its rarity's
/// color, its outline and its icon placed by its settings
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PiecePreviewAttribute : Attribute
{
    public readonly float cellSize;

    /// <param name="cellSize">The size of a cell in the preview, in points</param>
    public PiecePreviewAttribute(float cellSize = 48) => this.cellSize = cellSize;
}
