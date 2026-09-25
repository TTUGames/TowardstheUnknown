using UnityEngine;
using UnityEngine.EventSystems;

public class InfoEntity : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //The panel being shared by all entities, only the hovered one displays it
    private static InfoEntity hoveredEntity;

    private string entityName;
    private EnemyStats enemyStats;

    public void Start()
    {
        enemyStats = GetComponent<EnemyStats>();
        entityName = Localization.Entity(gameObject.name.Replace("(Clone)", ""));
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
    }

    /// <summary>
    /// Updates the panel if this entity is hovered, hides it if the entity died
    /// </summary>
    public void Refresh()
    {
        if (hoveredEntity == this) Display();
    }

    private void Display()
    {
        EntityInfoPanel panel = GameScene.UI.Hud.EntityInfo;
        if (GameScene.UI.IsMenuOpen || enemyStats.CurrentHealth <= 0)
            panel.Hide();
        else
            panel.Show(transform.position, entityName, enemyStats.CurrentHealth, enemyStats.maxMovementPoints);
    }
}
