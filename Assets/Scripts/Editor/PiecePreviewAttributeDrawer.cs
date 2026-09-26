using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws the artifact's piece under the field, as the inventory shows it, without its animation: to tune its icon's fit,
/// scale, rotation and offset by eye
/// </summary>
public class PiecePreviewAttributeDrawer : OdinAttributeDrawer<PiecePreviewAttribute>
{
    private static readonly Color Background = new(0.1f, 0.1f, 0.13f);
    private static readonly Color Line = new(1, 1, 1, 0.5f);

    protected override void DrawPropertyLayout(GUIContent label)
    {
        CallNextDrawer(label);
        if (Property.Tree.WeakTargets.Count == 0 || Property.Tree.WeakTargets[0] is not ArtifactData artifact) return;
        if (artifact.shape == null || artifact.shape.Count == 0) return;

        float cellSize = Attribute.cellSize;
        var cells = new HashSet<Vector2Int>(artifact.shape);
        Vector2Int size = ArtifactPieceLayout.Size(cells);
        const float padding = 12;
        Rect area = GUILayoutUtility.GetRect(size.x * cellSize + padding * 2, size.y * cellSize + padding * 2, GUILayout.ExpandWidth(false));
        area.x += EditorGUIUtility.labelWidth;
        if (Event.current.type != EventType.Repaint) return;

        EditorGUI.DrawRect(area, Background);
        var origin = new Vector2(area.x + padding, area.y + padding);
        RarityPalette palette = Resources.Load<GameAssets>("GameAssets")?.rarityPalette;
        Color surface = palette != null ? palette.Get(artifact.rarity, RarityPalette.Tone.Surface) : Color.gray;

        Sprite icon = artifact.skillBarIcon;
        Texture2D texture = icon != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(icon)) : null;
        ArtifactPieceLayout.IconPlacement placement = texture != null
            ? ArtifactPieceLayout.PlaceIcon(cells, size, cellSize, icon, artifact.inventoryIconBounds, artifact.inventoryIconFit,
                artifact.inventoryIconScale, artifact.inventoryIconRotation, artifact.inventoryIconOffset)
            : default;

        foreach (Vector2Int cell in cells)
        {
            Rect rect = ArtifactPieceLayout.CellRect(cells, size, cell, cellSize);
            rect.position += origin;
            EditorGUI.DrawRect(rect, surface);
            if (texture == null) continue;
            // The part of the icon over this cell, turned around its pivot
            GUI.BeginClip(rect);
            Matrix4x4 matrix = GUI.matrix;
            Vector2 topLeft = placement.rect.position + origin - rect.position;
            GUIUtility.RotateAroundPivot(placement.rotation, topLeft + placement.pivot);
            GUI.DrawTexture(new Rect(topLeft, placement.rect.size), texture, ScaleMode.StretchToFill);
            GUI.matrix = matrix;
            GUI.EndClip();
        }

        Handles.color = Line;
        foreach (List<Vector2> loop in ArtifactPieceLayout.Outline(cells, size, cellSize, ArtifactPieceLayout.Gap + 0.5f))
        {
            var points = new Vector3[loop.Count + 1];
            for (int i = 0; i <= loop.Count; i++) points[i] = loop[i % loop.Count] + origin;
            Handles.DrawAAPolyLine(1.5f, points);
        }
    }
}
