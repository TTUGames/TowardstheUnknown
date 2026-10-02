using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Labels of the HUD shown over points of the world: the combat popups, the damage previews and the queued cast markers.
/// Keeps the labels of one class, created as they are needed and hidden when unused
/// </summary>
public sealed class WorldLabels
{
    private readonly VisualElement root;
    private readonly string className;
    private readonly List<Label> labels = new();

    public WorldLabels(VisualElement root, string className)
    {
        this.root = root;
        this.className = className;
    }

    public int Count => labels.Count;
    public Label this[int index] => labels[index];

    /// <summary>
    /// Shows the label at <paramref name="index"/>, created with those before it if needed
    /// </summary>
    public Label Show(int index)
    {
        while (labels.Count <= index)
        {
            var created = new Label { pickingMode = PickingMode.Ignore };
            created.AddToClassList(className);
            root.Add(created);
            labels.Add(created);
        }
        labels[index].style.display = DisplayStyle.Flex;
        return labels[index];
    }

    /// <summary>
    /// Hides the labels from <paramref name="index"/> on
    /// </summary>
    public void HideFrom(int index)
    {
        for (int i = index; i < labels.Count; i++)
            labels[i].style.display = DisplayStyle.None;
    }

    /// <summary>
    /// If the labels can be placed: the panel and the camera exist
    /// </summary>
    public static bool CanPlace(VisualElement root) => root.panel != null && Camera.main != null;

    /// <summary>
    /// Places <paramref name="label"/>, a child of <paramref name="root"/>, <paramref name="offsetUp"/> panel points above the world point
    /// </summary>
    public static void Place(VisualElement root, VisualElement label, Vector3 world, float offsetUp)
    {
        Vector2 position = root.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, world, Camera.main));
        label.style.left = position.x;
        label.style.top = position.y - offsetUp;
    }
}
