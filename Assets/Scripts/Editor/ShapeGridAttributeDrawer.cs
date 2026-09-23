using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Paints a shape cell by cell: click to toggle a cell, drag to paint several. The y axis goes up, as in the inventory.
/// </summary>
public class ShapeGridAttributeDrawer : OdinAttributeDrawer<ShapeGridAttribute, List<Vector2Int>>
{
    private const float CellSize = 28f;
    private const float Spacing = 2f;

    private static readonly Color EmptyColor = new Color(0.16f, 0.16f, 0.16f);
    private static readonly Color FilledColor = new Color(0.45f, 0.35f, 0.85f, 0.75f);
    private static readonly Color HoverColor = new Color(1f, 1f, 1f, 0.15f);

    private bool isPainting;
    private bool paintValue;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        List<Vector2Int> shape = ValueEntry.SmartValue ?? new List<Vector2Int>();
        int size = Attribute.size;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label ?? GUIContent.none);
        EditorGUILayout.LabelField(shape.Count + " cells", EditorStyles.miniLabel, GUILayout.Width(50));
        if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
            Apply(new List<Vector2Int>());
        EditorGUILayout.EndHorizontal();

        float side = size * CellSize;
        Rect grid = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth + side, side, GUILayout.ExpandWidth(false));
        grid.x += EditorGUIUtility.labelWidth;
        EditorGUIUtility.AddCursorRect(grid, MouseCursor.Link);

        Rect CellRect(int x, int y) => new Rect(grid.x + x * CellSize, grid.y + (size - 1 - y) * CellSize, CellSize - Spacing, CellSize - Spacing);

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                EditorGUI.DrawRect(CellRect(x, y), EmptyColor);

        DrawSprite(shape, CellRect);

        Event current = Event.current;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Rect rect = CellRect(x, y);
                if (shape.Contains(cell)) EditorGUI.DrawRect(rect, FilledColor);
                if (!rect.Contains(current.mousePosition)) continue;

                EditorGUI.DrawRect(rect, HoverColor);
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    isPainting = true;
                    paintValue = !shape.Contains(cell);
                    Paint(shape, cell);
                    current.Use();
                }
                else if (current.type == EventType.MouseDrag && isPainting)
                {
                    Paint(shape, cell);
                    current.Use();
                }
            }
        }

        if (current.type == EventType.MouseUp) isPainting = false;
        if (grid.Contains(current.mousePosition)) GUIHelper.RequestRepaint();
    }

    /// <summary>
    /// Draws the sprite over the shape's bounds, stretched like the inventory does
    /// </summary>
    private void DrawSprite(List<Vector2Int> shape, System.Func<int, int, Rect> cellRect)
    {
        if (Attribute.spriteMember == null || shape.Count == 0) return;
        Sprite sprite = Property.Parent.Children[Attribute.spriteMember]?.ValueEntry.WeakSmartValue as Sprite;
        if (sprite == null) return;

        int width = shape.Max(cell => cell.x) + 1;
        int height = shape.Max(cell => cell.y) + 1;
        Rect bottomLeft = cellRect(0, 0);
        Rect topRight = cellRect(width - 1, height - 1);
        Rect bounds = Rect.MinMaxRect(bottomLeft.xMin, topRight.yMin, topRight.xMax, bottomLeft.yMax);

        Rect uv = sprite.textureRect;
        Texture2D texture = sprite.texture;
        GUI.DrawTextureWithTexCoords(bounds, texture, new Rect(uv.x / texture.width, uv.y / texture.height, uv.width / texture.width, uv.height / texture.height));
    }

    private void Paint(List<Vector2Int> shape, Vector2Int cell)
    {
        if (shape.Contains(cell) == paintValue) return;
        List<Vector2Int> painted = new List<Vector2Int>(shape);
        if (paintValue) painted.Add(cell);
        else painted.Remove(cell);
        Apply(painted);
    }

    private void Apply(List<Vector2Int> shape)
    {
        ValueEntry.SmartValue = shape;
    }
}
