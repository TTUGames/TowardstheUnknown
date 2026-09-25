using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Shows the enemy's info panel while the pointer is over it, following its stats
/// </summary>
public class InfoEntity : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //The panel being shared by all entities, only the hovered one displays it
    private static InfoEntity hoveredEntity;

    private string entityName;
    private EnemyStats enemyStats;

    public void Start()
    {
        //Only the enemies show their info (the player of the test scenes carries one)
        if (!TryGetComponent(out enemyStats))
        {
            enabled = false;
            return;
        }
        entityName = Localization.Entity(enemyStats.ID);
        enemyStats.StatsChanged += Refresh;
    }

    //Needs a PhysicsRaycaster on the camera
    public void OnPointerEnter(PointerEventData eventData)
    {
        hoveredEntity = this;
        Display();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoveredEntity != this) return;
        hoveredEntity = null;
        GameScene.UI.Hud.EntityInfo.Hide();
    }

    private void OnDestroy()
    {
        if (hoveredEntity == this) hoveredEntity = null;
        if (enemyStats != null) enemyStats.StatsChanged -= Refresh;
    }

    /// <summary>
    /// Updates the panel if this entity is hovered, hides it if the entity died
    /// </summary>
    private void Refresh()
    {
        if (hoveredEntity == this) Display();
    }

    private void Display()
    {
        EntityInfoPanel panel = GameScene.UI.Hud.EntityInfo;
        if (GameScene.IsGameplayBlocked || enemyStats.CurrentHealth <= 0)
            panel.Hide();
        else
            panel.Show(transform.position, entityName, enemyStats.CurrentHealth, enemyStats.maxMovementPoints);
    }
}
