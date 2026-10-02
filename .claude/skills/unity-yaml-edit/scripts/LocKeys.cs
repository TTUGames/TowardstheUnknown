using UnityEditor;
using UnityEditor.Localization;

// The string table entries through the Localization editor API, which writes the tables' YAML with small diffs:
//   unity --json command run_script --file .claude/skills/unity-yaml-edit/scripts/LocKeys.cs --entry LocKeys.Set --args '["UI","Key","Texte","Text"]'
//   LocKeys.Get '["UI","Key"]', LocKeys.Remove '["UI","Key"]'. Edit mode only (it saves the assets)
public static class LocKeys
{
    // Adds (or rewrites) a key of a string table collection, in French and English, and saves the tables
    public static string Set(string collection, string key, string fr, string en)
    {
        var tables = LocalizationEditorSettings.GetStringTableCollection(collection);
        if (tables == null) return "no collection " + collection;
        if (!tables.SharedData.Contains(key)) tables.SharedData.AddKey(key);
        foreach (var table in tables.StringTables)
        {
            string code = table.LocaleIdentifier.Code;
            table.AddEntry(key, code.StartsWith("fr") ? fr : en);
            EditorUtility.SetDirty(table);
        }
        EditorUtility.SetDirty(tables.SharedData);
        AssetDatabase.SaveAssets();
        return $"{collection}/{key} set";
    }

    public static string Remove(string collection, string key)
    {
        var tables = LocalizationEditorSettings.GetStringTableCollection(collection);
        if (!tables.SharedData.Contains(key)) return "no key " + key;
        tables.RemoveEntry(key);
        foreach (var table in tables.StringTables) EditorUtility.SetDirty(table);
        EditorUtility.SetDirty(tables.SharedData);
        AssetDatabase.SaveAssets();
        return key + " removed";
    }

    public static string Get(string collection, string key)
    {
        var tables = LocalizationEditorSettings.GetStringTableCollection(collection);
        string result = key + ":";
        foreach (var table in tables.StringTables)
            result += $" {table.LocaleIdentifier.Code}=\"{table.GetEntry(key)?.Value}\"";
        return result;
    }
}
