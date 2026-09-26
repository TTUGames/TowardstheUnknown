using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Measures where the drawing lies in each artifact's skill bar icon (its transparent margin left out), for the pieces
/// of the inventory to frame the drawing itself. Run it after changing an icon
/// </summary>
public static class ArtifactIconTools
{
    private const float AlphaThreshold = 0.1f;

    [MenuItem("Tools/Artifacts/Measure Icons")]
    public static void MeasureIcons()
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:ArtifactData"))
        {
            var artifact = AssetDatabase.LoadAssetAtPath<ArtifactData>(AssetDatabase.GUIDToAssetPath(guid));
            if (artifact.skillBarIcon == null) continue;
            Rect bounds = Measure(artifact.skillBarIcon);
            if (bounds == artifact.inventoryIconBounds) continue;
            Undo.RecordObject(artifact, "Measure Icons");
            artifact.inventoryIconBounds = bounds;
            EditorUtility.SetDirty(artifact);
            count++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Measured the icons of {count} artifacts");
    }

    /// <summary>
    /// The bounds of the sprite's opaque pixels, from 0 to 1 from its bottom left corner. Reads the sprite's source file:
    /// the imported texture is not readable, and in the editor the sprite may point to its packed atlas
    /// </summary>
    public static Rect Measure(Sprite sprite)
    {
        string path = AssetDatabase.GetAssetPath(sprite);
        var texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(path));
        // The sprite's rect is in the imported texture's pixels, which may be scaled from the source's
        float scale = (float)texture.width / AssetDatabase.LoadAssetAtPath<Texture2D>(path).width;
        Rect rect = sprite.rect;
        int x0 = (int)(rect.x * scale), y0 = (int)(rect.y * scale), width = (int)(rect.width * scale), height = (int)(rect.height * scale);
        int minX = width, minY = height, maxX = -1, maxY = -1;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (texture.GetPixel(x0 + x, y0 + y).a < AlphaThreshold) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        Object.DestroyImmediate(texture);
        if (maxX < 0) return new Rect(0, 0, 1, 1);
        return Rect.MinMaxRect((float)minX / width, (float)minY / height, (float)(maxX + 1) / width, (float)(maxY + 1) / height);
    }
}
