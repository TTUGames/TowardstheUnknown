using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

/// <summary>
/// The Classic's UI sheets (<see cref="GameAssets.classicSheets"/>), on the roots of the UI documents while the Classic is shown,
/// and the Classic's images of some elements (the original's inventory pieces). They are addressable and nothing the Anniversary
/// loads references them: loaded with the Classic, released with it, and their textures (the original's UI, ~350 MB) with them
/// </summary>
public static class ClassicStyles
{
    private static readonly List<VisualElement> roots = new();
    private static readonly List<AsyncOperationHandle<StyleSheet>> handles = new();
    private static readonly Dictionary<VisualElement, AssetReferenceSprite> images = new();
    private static readonly Dictionary<string, AsyncOperationHandle<Sprite>> sprites = new();
    private static bool listening;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        roots.Clear();
        handles.Clear();
        images.Clear();
        sprites.Clear();
        listening = false;
    }

    /// <summary>
    /// Styles the root with the Classic's sheets while the Classic is shown, until <see cref="Detach"/>
    /// </summary>
    public static void Attach(VisualElement root)
    {
        Listen();
        if (roots.Contains(root)) return;
        roots.Add(root);
        if (Edition.IsClassic) AddSheets(root);
    }

    public static void Detach(VisualElement root)
    {
        if (!roots.Remove(root)) return;
        RemoveSheets(root);
    }

    /// <summary>
    /// Gives the element the Classic's image as its background while the Classic is shown, none in the Anniversary, as long as
    /// it is on a panel
    /// </summary>
    public static void Image(VisualElement element, AssetReferenceSprite image)
    {
        Listen();
        element.RegisterCallback<AttachToPanelEvent>(_ => ShowImage(element, image));
        element.RegisterCallback<DetachFromPanelEvent>(_ => images.Remove(element));
        if (element.panel != null) ShowImage(element, image);
    }

    private static void ShowImage(VisualElement element, AssetReferenceSprite image)
    {
        images[element] = image;
        element.style.backgroundImage = Edition.IsClassic ? new StyleBackground(LoadSprite(image)) : StyleKeyword.Null;
    }

    private static Sprite LoadSprite(AssetReferenceSprite image)
    {
        string key = image.AssetGUID;
        if (!sprites.TryGetValue(key, out AsyncOperationHandle<Sprite> handle))
        {
            handle = Addressables.LoadAssetAsync<Sprite>(image);
            handle.WaitForCompletion();
            sprites[key] = handle;
        }
        return handle.Result;
    }

    private static void Listen()
    {
        if (listening) return;
        listening = true;
        Edition.Changed += OnEditionChanged;
    }

    private static void OnEditionChanged(GameEdition edition)
    {
        if (edition == GameEdition.Classic)
        {
            foreach (VisualElement root in roots) AddSheets(root);
            foreach (KeyValuePair<VisualElement, AssetReferenceSprite> image in images)
                image.Key.style.backgroundImage = new StyleBackground(LoadSprite(image.Value));
            return;
        }
        foreach (VisualElement root in roots) RemoveSheets(root);
        foreach (AsyncOperationHandle<StyleSheet> handle in handles) Addressables.Release(handle);
        handles.Clear();
        foreach (VisualElement element in images.Keys) element.style.backgroundImage = StyleKeyword.Null;
        foreach (AsyncOperationHandle<Sprite> handle in sprites.Values) Addressables.Release(handle);
        sprites.Clear();
    }

    // Loaded at once: a screen shown in the Classic never shows a frame of the Anniversary's style
    private static void Load()
    {
        if (handles.Count != 0) return;
        foreach (AssetReferenceT<StyleSheet> sheet in GameAssets.Instance.classicSheets)
        {
            AsyncOperationHandle<StyleSheet> handle = Addressables.LoadAssetAsync<StyleSheet>(sheet);
            handle.WaitForCompletion();
            handles.Add(handle);
        }
    }

    private static void AddSheets(VisualElement root)
    {
        Load();
        foreach (AsyncOperationHandle<StyleSheet> handle in handles)
            if (handle.Result != null && !root.styleSheets.Contains(handle.Result)) root.styleSheets.Add(handle.Result);
    }

    private static void RemoveSheets(VisualElement root)
    {
        foreach (AsyncOperationHandle<StyleSheet> handle in handles)
            if (handle.IsValid() && handle.Result != null) root.styleSheets.Remove(handle.Result);
    }
}
