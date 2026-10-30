using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sets the Game view to a fixed resolution (Screen.width / height in Play mode), adding it to the Standalone sizes if it is
/// missing, to check the game on another screen shape (16:10, ultra-wide, 4:3). Through the editor's internal GameViewSizes
/// and GameView, by reflection. Run through the unity CLI (script run), in or out of Play mode
/// </summary>
public static class GameViewSize
{
    private static readonly Assembly editor = typeof(Editor).Assembly;

    /// <summary>
    /// Sets the Game view to width x height ("2560", "1600")
    /// </summary>
    public static string Set(int width, int height)
    {
        object group = Group();
        int index = Find(group, width, height);
        if (index < 0)
        {
            Type sizeType = editor.GetType("UnityEditor.GameViewSize");
            Type kindType = editor.GetType("UnityEditor.GameViewSizeType");
            object size = Activator.CreateInstance(sizeType, Enum.Parse(kindType, "FixedResolution"), width, height, $"{width}x{height}");
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            index = Find(group, width, height);
        }
        Type gameViewType = editor.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        PropertyInfo selected = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        selected.SetValue(gameView, index);
        gameView.Repaint();
        return $"game view {width}x{height} (index {index})";
    }

    /// <summary>
    /// The size the game sees
    /// </summary>
    public static string Show() => $"screen {Screen.width}x{Screen.height}";

    private static object Group()
    {
        Type sizesType = editor.GetType("UnityEditor.GameViewSizes");
        object sizes = sizesType.BaseType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        return sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { GameViewSizeGroupType.Standalone });
    }

    private static int Find(object group, int width, int height)
    {
        Type groupType = group.GetType();
        int count = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
        MethodInfo get = groupType.GetMethod("GetGameViewSize");
        for (int i = 0; i < count; i++)
        {
            object size = get.Invoke(group, new object[] { i });
            Type sizeType = size.GetType();
            if ((int)sizeType.GetProperty("width").GetValue(size) == width && (int)sizeType.GetProperty("height").GetValue(size) == height
                && sizeType.GetProperty("sizeType").GetValue(size).ToString() == "FixedResolution")
                return i;
        }
        return -1;
    }
}
