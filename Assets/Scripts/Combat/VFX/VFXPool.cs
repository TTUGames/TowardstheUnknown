using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Reuses the VFX instances instead of instantiating and destroying them on each play.
/// A released instance is deactivated under a pool object; getting it back reactivates it, which restarts its particle systems and VFX graphs
/// </summary>
public static class VFXPool
{
    private static readonly Dictionary<GameObject, Stack<GameObject>> free = new Dictionary<GameObject, Stack<GameObject>>();
    private static readonly Dictionary<GameObject, GameObject> prefabOf = new Dictionary<GameObject, GameObject>();
    private static Transform root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        free.Clear();
        prefabOf.Clear();
        root = null;
    }

    /// <summary>
    /// The pool object, created in the active scene: its instances die with the scene
    /// </summary>
    private static Transform Root
    {
        get
        {
            if (root != null) return root;
            //A new scene destroyed the previous pool
            free.Clear();
            prefabOf.Clear();
            root = new GameObject("VFX Pool").transform;
            root.gameObject.SetActive(false);
            return root;
        }
    }

    /// <summary>
    /// An active instance of the prefab under the parent, placed like the prefab
    /// </summary>
    public static GameObject Get(GameObject prefab, Transform parent)
    {
        GameObject instance = Take(prefab);
        if (instance == null)
        {
            instance = Object.Instantiate(prefab, parent);
            prefabOf[instance] = prefab;
            return instance;
        }
        instance.transform.SetParent(parent, false);
        instance.transform.SetLocalPositionAndRotation(prefab.transform.localPosition, prefab.transform.localRotation);
        instance.transform.localScale = prefab.transform.localScale;
        instance.SetActive(true);
        Restart(instance);
        return instance;
    }

    /// <summary>
    /// Plays the effects from their start: a reactivated particle system would otherwise resume where it stopped
    /// </summary>
    private static void Restart(GameObject instance)
    {
        foreach (ParticleSystem system in instance.GetComponentsInChildren<ParticleSystem>())
        {
            system.Clear(false);
            if (system.main.playOnAwake) system.Play(false);
        }
        foreach (VisualEffect effect in instance.GetComponentsInChildren<VisualEffect>())
            effect.Reinit();
    }

    /// <summary>
    /// An active instance of the prefab in the world, not parented
    /// </summary>
    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = Get(prefab, null);
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    private static GameObject Take(GameObject prefab)
    {
        Transform pool = Root;
        if (!free.TryGetValue(prefab, out Stack<GameObject> instances)) return null;
        while (instances.Count > 0)
        {
            GameObject instance = instances.Pop();
            if (instance != null && instance.transform.parent == pool) return instance;
        }
        return null;
    }

    /// <summary>
    /// Deactivates the instance and keeps it for the next play. An instance not made by the pool is destroyed
    /// </summary>
    public static void Release(GameObject instance)
    {
        if (instance == null) return;
        if (!prefabOf.TryGetValue(instance, out GameObject prefab) || prefab == null)
        {
            Object.Destroy(instance);
            return;
        }
        instance.SetActive(false);
        instance.transform.SetParent(Root, false);
        if (!free.TryGetValue(prefab, out Stack<GameObject> instances))
            free[prefab] = instances = new Stack<GameObject>();
        instances.Push(instance);
    }

    /// <summary>
    /// Releases the instance after a delay, run by the action manager
    /// </summary>
    public static void Release(GameObject instance, float delay)
    {
        ActionManager.Run(ReleaseAfter(instance, delay));
    }

    private static IEnumerator ReleaseAfter(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        Release(instance);
    }
}
