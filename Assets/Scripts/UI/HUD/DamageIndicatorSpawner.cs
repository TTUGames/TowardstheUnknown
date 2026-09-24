using UnityEngine;

/// <summary>
/// Shows a damage indicator over each entity that takes damage. Put it on the HUD canvas.
/// </summary>
public class DamageIndicatorSpawner : MonoBehaviour
{
    [SerializeField] private DamageIndicator indicatorPrefab;

    private Camera cam;

    private void Awake() {
        cam = Camera.main;
    }

    private void OnEnable() {
        EntityStats.AnyDamageTaken += Spawn;
    }

    private void OnDisable() {
        EntityStats.AnyDamageTaken -= Spawn;
    }

    private void Spawn(EntityStats entity, int damage) {
        DamageIndicator indicator = Instantiate(indicatorPrefab);
        indicator.transform.SetParent(transform);
        indicator.Show(damage, cam.WorldToScreenPoint(entity.transform.position));
    }
}
