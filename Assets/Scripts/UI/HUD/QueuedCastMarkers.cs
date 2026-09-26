using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Over each target of the player's queued casts: their order in the queue, following the targets while they move
/// </summary>
public class QueuedCastMarkers : IDisposable
{
    // Above the targeted entity or tile, in panel points
    private const float OffsetUp = 100;

    private readonly VisualElement root;
    private readonly PlayerAttack attack;
    private readonly List<Label> labels = new();
    // The world point each shown label follows
    private readonly List<Func<Vector3>> anchors = new();
    private readonly IVisualElementScheduledItem follow;

    public QueuedCastMarkers(VisualElement root, PlayerAttack attack)
    {
        this.root = root;
        this.attack = attack;
        attack.QueueChanged += Show;
        follow = root.schedule.Execute(Place).Every(0);
        follow.Pause();
    }

    public void Dispose()
    {
        follow.Pause();
        if (attack != null) attack.QueueChanged -= Show;
    }

    /// <summary>
    /// One label per target, with the orders of the casts queued on it
    /// </summary>
    private void Show()
    {
        anchors.Clear();
        var texts = new List<string>();
        var keys = new List<object>();
        IReadOnlyList<PlayerAttack.QueuedCast> queued = attack.QueuedCasts;
        for (int i = 0; i < queued.Count; i++)
        {
            PlayerAttack.QueuedCast cast = queued[i];
            object key = cast.Target != null ? cast.Target : cast.Tile;
            int index = keys.IndexOf(key);
            if (index < 0)
            {
                keys.Add(key);
                texts.Add((i + 1).ToString());
                Transform anchor = cast.Target != null ? cast.Target.transform : cast.Tile.transform;
                anchors.Add(() => anchor != null ? anchor.position : Vector3.positiveInfinity);
            }
            else texts[index] += " " + (i + 1);
        }

        while (labels.Count < texts.Count)
        {
            var created = new Label { pickingMode = PickingMode.Ignore };
            created.AddToClassList("queued-cast");
            root.Add(created);
            labels.Add(created);
        }
        for (int i = 0; i < labels.Count; i++)
        {
            bool shown = i < texts.Count;
            labels[i].style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            if (shown) labels[i].text = texts[i];
        }
        Place();
        if (texts.Count > 0) follow.Resume();
        else follow.Pause();
    }

    private void Place()
    {
        if (root.panel == null || Camera.main == null) return;
        for (int i = 0; i < anchors.Count; i++)
        {
            Vector3 world = anchors[i]();
            if (float.IsInfinity(world.x))
            {
                labels[i].style.display = DisplayStyle.None;
                continue;
            }
            Vector2 position = root.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, world, Camera.main));
            labels[i].style.left = position.x;
            labels[i].style.top = position.y - OffsetUp;
        }
    }
}
