using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

/// <summary>
/// Puts the addressable Classic sides of ClassicSkin (their classicAsset) in the ClassicSkin Addressables group, made with
/// the ClassicUI group's settings if missing, and takes out of it what no pair names any more. Run by build_skin.py
/// </summary>
public static class SkinGroup
{
    private const string SkinPath = "Assets/Data/Editions/ClassicSkin.asset";
    private const string GroupName = "ClassicSkin";

    public static string Sync()
    {
        AssetDatabase.ImportAsset(SkinPath, ImportAssetOptions.ForceUpdate);
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetGroup group = settings.FindGroup(GroupName);
        if (group == null)
        {
            AddressableAssetGroup template = settings.FindGroup("ClassicUI");
            group = settings.CreateGroup(GroupName, false, false, true, template.Schemas, template.SchemaTypes.ToArray());
        }
        var wanted = new HashSet<string>();
        var skin = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(SkinPath));
        foreach (string list in new[] { "materials", "prefabs" })
        {
            SerializedProperty pairs = skin.FindProperty(list);
            for (int i = 0; i < pairs.arraySize; i++)
            {
                string guid = pairs.GetArrayElementAtIndex(i).FindPropertyRelative("classicAsset.m_AssetGUID").stringValue;
                if (!string.IsNullOrEmpty(guid)) wanted.Add(guid);
            }
        }
        int added = 0, removed = 0;
        foreach (string guid in wanted)
        {
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry != null && entry.parentGroup == group) continue;
            settings.CreateOrMoveEntry(guid, group, false, false);
            added++;
        }
        foreach (AddressableAssetEntry entry in group.entries.ToList())
            if (!wanted.Contains(entry.guid))
            {
                group.RemoveAssetEntry(entry, false);
                removed++;
            }
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        AssetDatabase.SaveAssets();
        return $"ClassicSkin group: {group.entries.Count} entries ({added} added, {removed} removed)";
    }
}
