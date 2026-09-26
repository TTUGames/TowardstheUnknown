using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The enemies' deaths in particles of their energy: a burst from the body when the lethal hit lands, then motes rising
/// from the corpse as it vanishes (<see cref="EntityFeedback.VanishStarted"/>). Tinted by the entity's energy color,
/// the <c>_GlowColor</c> of its materials, and sized by its body
/// </summary>
public class DeathFeedback : MonoBehaviour
{
    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

    [SerializeField, Required, AssetsOnly, Tooltip("At the middle of the body when it dies; its shape's radius is scaled by the body's width")] private ParticleSystem burst;
    [SerializeField, Required, AssetsOnly, Tooltip("From the corpse as it vanishes; its box shape takes the body's size")] private ParticleSystem dissipation;
    [SerializeField, ColorUsage(false), Tooltip("For an entity without an energy color. The particles' material holds the brightness: their colors are clamped to 1")] private Color fallbackColor = new(0.9f, 0.9f, 1f);

    private void OnEnable()
    {
        GameEvents.EntityDied += OnEntityDied;
        EntityFeedback.VanishStarted += OnVanishStarted;
    }

    private void OnDisable()
    {
        GameEvents.EntityDied -= OnEntityDied;
        EntityFeedback.VanishStarted -= OnVanishStarted;
    }

    private void OnEntityDied(EntityStats entity)
    {
        if (entity.type == EntityType.PLAYER) return;
        Bounds body = BodyBounds(entity.gameObject);
        Play(burst, body.center, body, EnergyColor(entity.gameObject));
    }

    private void OnVanishStarted(EntityFeedback corpse, float duration)
    {
        if (!corpse.TryGetComponent(out EntityStats entity) || entity.type == EntityType.PLAYER) return;
        Bounds body = BodyBounds(corpse.gameObject);
        Play(dissipation, body.center, body, EnergyColor(corpse.gameObject));
    }

    private void Play(ParticleSystem prefab, Vector3 position, Bounds body, Color color)
    {
        GameObject instance = VFXPool.Get(prefab.gameObject, position, Quaternion.identity);
        float lifetime = 0;
        foreach (ParticleSystem system in instance.GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.MainModule main = system.main;
            main.startColor = color;
            ParticleSystem.ShapeModule shape = system.shape;
            if (shape.enabled)
            {
                Vector3 size = body.size;
                if (shape.shapeType == ParticleSystemShapeType.Box) shape.scale = new Vector3(size.x * 0.7f, size.y * 0.8f, size.z * 0.7f);
                else shape.radius = Mathf.Max(size.x, size.z) * 0.4f;
            }
            system.Clear();
            system.Play();
            lifetime = Mathf.Max(lifetime, main.duration + main.startLifetime.constantMax);
        }
        VFXPool.Release(instance, lifetime);
    }

    // The meshes' bounds: the entity's transform stands at its feet
    private static Bounds BodyBounds(GameObject entity)
    {
        Bounds bounds = new(entity.transform.position + Vector3.up * 0.5f, Vector3.one * 0.5f);
        bool found = false;
        foreach (Renderer renderer in entity.GetComponentsInChildren<Renderer>())
        {
            if (renderer is not SkinnedMeshRenderer && renderer is not MeshRenderer) continue;
            if (found) bounds.Encapsulate(renderer.bounds);
            else bounds = renderer.bounds;
            found = true;
        }
        return bounds;
    }

    private Color EnergyColor(GameObject entity)
    {
        foreach (Renderer renderer in entity.GetComponentsInChildren<Renderer>())
            foreach (Material material in renderer.sharedMaterials)
                if (material != null && material.HasColor(GlowColor))
                {
                    Color color = material.GetColor(GlowColor);
                    float brightest = Mathf.Max(color.r, color.g, color.b);
                    return brightest > 0 ? color / brightest : fallbackColor;
                }
        return fallbackColor;
    }
}
