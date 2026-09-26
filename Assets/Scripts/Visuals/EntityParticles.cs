using UnityEngine;

/// <summary>
/// Plays a particle prefab on an entity's body, from the VFX pool: tinted, its shape sized by the body's bounds (a box
/// takes the body's size, other shapes a radius from its width). Shared by <see cref="DeathFeedback"/> and <see cref="RecoveryFeedback"/>
/// </summary>
public static class EntityParticles
{
    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

    /// <summary>
    /// Plays the prefab at the middle of the body, released once its particles are gone
    /// </summary>
    public static void Play(ParticleSystem prefab, GameObject entity, Color color)
    {
        Bounds body = BodyBounds(entity);
        GameObject instance = VFXPool.Get(prefab.gameObject, body.center, Quaternion.identity);
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

    /// <summary>
    /// The meshes' bounds: the entity's transform stands at its feet
    /// </summary>
    public static Bounds BodyBounds(GameObject entity)
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

    /// <summary>
    /// The entity's energy color, the <c>_GlowColor</c> of its first material having one, brought to a brightest channel
    /// of 1 (the particles' colors are clamped to 1: their material holds the brightness); the fallback without one
    /// </summary>
    public static Color EnergyColor(GameObject entity, Color fallback)
    {
        foreach (Renderer renderer in entity.GetComponentsInChildren<Renderer>())
            foreach (Material material in renderer.sharedMaterials)
                if (material != null && material.HasColor(GlowColor))
                {
                    Color color = material.GetColor(GlowColor);
                    float brightest = Mathf.Max(color.r, color.g, color.b);
                    return brightest > 0 ? color / brightest : fallback;
                }
        return fallback;
    }
}
