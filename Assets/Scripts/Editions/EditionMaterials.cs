using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gives the renderers the materials of the current edition, from the pairs of the <see cref="EditionSkin"/>.
/// Applied to each scene as it loads (<see cref="Edition"/>), to what code instantiates (the rooms, the enemies, the pooled VFX),
/// and to every loaded scene when the edition changes, inactive objects included (the rooms left, the pool)
/// </summary>
public static class EditionMaterials
{
    private static readonly List<Material> buffer = new();
    private static readonly List<GameObject> roots = new();

    /// <summary>
    /// Swaps the materials with a pair on every renderer under <paramref name="root"/>, particles included
    /// </summary>
    public static void Apply(GameObject root)
    {
        EditionSkin skin = GameAssets.Instance.classicSkin;
        if (root == null || skin == null || !skin.HasMaterials) return;
        GameEdition edition = Edition.Current;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.GetSharedMaterials(buffer);
            bool changed = false;
            for (int i = 0; i < buffer.Count; i++)
            {
                Material material = skin.Resolve(buffer[i], edition);
                if (material == buffer[i]) continue;
                buffer[i] = material;
                changed = true;
            }
            if (changed) renderer.SetSharedMaterials(buffer);
        }
    }

    public static void Apply(Scene scene)
    {
        scene.GetRootGameObjects(roots);
        foreach (GameObject root in roots) Apply(root);
    }

    public static void ApplyToLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded) Apply(scene);
        }
    }
}
