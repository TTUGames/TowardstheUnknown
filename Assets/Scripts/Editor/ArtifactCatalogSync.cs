using System.Linq;
using UnityEditor;

/// <summary>
/// Keeps every <see cref="ArtifactCatalog"/> listing all the <see cref="ArtifactData"/> assets, by name, after any import,
/// deletion or move of an asset
/// </summary>
public class ArtifactCatalogSync : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (imported.Concat(deleted).Concat(moved).Any(path => path.EndsWith(".asset"))) Sync();
    }

    [MenuItem("Tools/Artifacts/Sync Catalog")]
    public static void Sync()
    {
        var all = AssetDatabase.FindAssets("t:ArtifactData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<ArtifactData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(artifact => artifact != null)
            .OrderBy(artifact => artifact.name, System.StringComparer.Ordinal)
            .ToList();
        foreach (string guid in AssetDatabase.FindAssets("t:ArtifactCatalog"))
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ArtifactCatalog>(AssetDatabase.GUIDToAssetPath(guid));
            if (catalog == null || catalog.artifacts.SequenceEqual(all)) continue;
            catalog.artifacts = all;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssetIfDirty(catalog);
        }
    }
}
