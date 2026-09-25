using UnityEngine;
using UnityEngine.EventSystems;

public class InfoEntity : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float upOffsetPercentage = 0.5f;
    [SerializeField] private float downOffsetPercentage = 0.5f;
    [SerializeField] private float leftOffsetPercentage = 0.02f;

    //The panel being shared by all entities, only the hovered one displays it
    private static InfoEntity hoveredEntity;

    private Camera cam;
    private string entityName;
    private EnemyStats enemyStats;

    public void Start()
    {
        cam = Camera.main;
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
        GameScene.UI.EntityInfoPanel.Hide();
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
        EntityInfoPanel panel = GameScene.UI.EntityInfoPanel;
        if (GameScene.UI.IsMenuOpen || enemyStats.CurrentHealth <= 0)
        {
            panel.Hide();
            return;
        }

        Vector3 entityScreenPosition = cam.WorldToScreenPoint(transform.position);
        if (entityScreenPosition.y > Screen.height / 2f)
            entityScreenPosition.y -= Screen.height * upOffsetPercentage;
        else
            entityScreenPosition.y += Screen.height * downOffsetPercentage;
        entityScreenPosition.x -= Screen.width * leftOffsetPercentage;
        panel.Show(entityScreenPosition, entityName, "<color=#e82a65>PV : " + enemyStats.CurrentHealth + " <color=#ffffff>|<color=#20D15F> PM : " + enemyStats.maxMovementPoints);
    }
}
