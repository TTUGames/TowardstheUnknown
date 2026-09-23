using System;

/// <summary>
/// Draws a list of cells as a grid painted cell by cell in the inspector, with an optional sprite shown behind the shape
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ShapeGridAttribute : Attribute
{
    public readonly int size;
    public readonly string spriteMember;

    /// <param name="size">The number of cells per side</param>
    /// <param name="spriteMember">The name of a sibling Sprite field drawn over the shape's bounds</param>
    public ShapeGridAttribute(int size, string spriteMember = null)
    {
        this.size = size;
        this.spriteMember = spriteMember;
    }
}
