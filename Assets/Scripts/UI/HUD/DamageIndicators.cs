using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Shows the damage taken by each entity over it: the number pops, stays, then fades out (transitions of Hud.uss)
/// </summary>
public class DamageIndicators : IDisposable
{
    private const long PopDelay = 170;
    private const long FadeDelay = 1000;
    private const long RemoveDelay = 1200;
    // Above the entity, in panel points
    private const float OffsetUp = 100;

    private readonly VisualElement root;

    public DamageIndicators(VisualElement root)
    {
        this.root = root;
        EntityStats.AnyDamageTaken += Spawn;
    }

    public void Dispose()
    {
        EntityStats.AnyDamageTaken -= Spawn;
    }

    private void Spawn(EntityStats entity, int damage)
    {
        Vector2 position = root.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, entity.transform.position, Camera.main));
        var indicator = new Label(damage.ToString()) { pickingMode = PickingMode.Ignore };
        indicator.AddToClassList("damage-indicator");
        indicator.style.left = position.x;
        indicator.style.top = position.y - OffsetUp;
        root.Add(indicator);
        indicator.schedule.Execute(() => indicator.AddToClassList("damage-indicator--shown")).StartingIn(PopDelay);
        indicator.schedule.Execute(() => indicator.AddToClassList("damage-indicator--fading")).StartingIn(FadeDelay);
        indicator.schedule.Execute(indicator.RemoveFromHierarchy).StartingIn(RemoveDelay);
    }
}
