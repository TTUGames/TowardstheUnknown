using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Wwise event and game parameter references, run through: unity --json command run_script --file <this file> --entry WwiseEvents.<Method> --args '[...]'

public static class WwiseEvents
{
    const string WorkUnit = "TowardstheUnknown_WwiseProject/Events/Default Work Unit.wwu";
    const string ParameterWorkUnit = "TowardstheUnknown_WwiseProject/Game Parameters/Default Work Unit.wwu";
    const string StateWorkUnit = "TowardstheUnknown_WwiseProject/States/Default Work Unit.wwu";

    static Dictionary<string, System.Guid> Objects(string workUnit, string element)
    {
        var objects = new Dictionary<string, System.Guid>();
        foreach (Match m in Regex.Matches(System.IO.File.ReadAllText(workUnit), $@"<{element} Name=""([^""]+)"" ID=""\{{([^}}]+)\}}"""))
            objects[m.Groups[1].Value] = new System.Guid(m.Groups[2].Value);
        return objects;
    }

    static Dictionary<string, System.Guid> Events() => Objects(WorkUnit, "Event");

    /// <summary>
    /// The events of the Wwise project whose name contains the filter (empty for all)
    /// </summary>
    public static string List(string filter)
    {
        return string.Join("\n", Events().Keys.Where(name => name.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(name => name));
    }

    /// <summary>
    /// Creates (or finds) the reference asset of each event, and prints the YAML of an AK.Wwise.Event field pointing to it
    /// </summary>
    public static string Reference(string[] names) => References(names, Events(), WwiseObjectType.Event);

    /// <summary>
    /// Creates (or finds) the reference asset of each game parameter, and prints the YAML of an AK.Wwise.RTPC field pointing to it
    /// </summary>
    public static string ReferenceParameter(string[] names) => References(names, Objects(ParameterWorkUnit, "GameParameter"), WwiseObjectType.GameParameter);

    /// <summary>
    /// Creates (or finds) the reference assets of states of a state group of the default states work unit, and prints the YAML of
    /// an AK.Wwise.State field pointing to each
    /// </summary>
    public static string ReferenceState(string group, string[] states)
    {
        var groupElement = System.Xml.Linq.XDocument.Load(StateWorkUnit).Descendants("StateGroup").FirstOrDefault(e => (string)e.Attribute("Name") == group);
        if (groupElement == null) return $"{group}: NOT IN THE WWISE PROJECT";
        var groupGuid = new System.Guid((string)groupElement.Attribute("ID"));
        var lines = new List<string>();
        foreach (string name in states)
        {
            var state = groupElement.Descendants("State").FirstOrDefault(e => (string)e.Attribute("Name") == name);
            if (state == null)
            {
                lines.Add($"{group}/{name}: NOT IN THE WWISE PROJECT");
                continue;
            }
            var reference = WwiseObjectReference.FindOrCreateWwiseObject(WwiseObjectType.State, name, new System.Guid((string)state.Attribute("ID")));
            ((WwiseGroupValueObjectReference)reference).SetupGroupObjectReference(group, groupGuid);
            EditorUtility.SetDirty(reference);
            string assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(reference));
            lines.Add($"{group}/{name}: reference {assetGuid}\n  <field>:\n    idInternal: 0\n    valueGuidInternal: \n    WwiseObjectReference: {{fileID: 11400000, guid: {assetGuid}, type: 2}}");
        }
        AssetDatabase.SaveAssets();
        return string.Join("\n", lines);
    }

    static string References(string[] names, Dictionary<string, System.Guid> objects, WwiseObjectType type)
    {
        var lines = new List<string>();
        foreach (string name in names)
        {
            if (!objects.TryGetValue(name, out System.Guid guid))
            {
                lines.Add($"{name}: NOT IN THE WWISE PROJECT");
                continue;
            }
            var reference = WwiseObjectReference.FindOrCreateWwiseObject(type, name, guid);
            string assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(reference));
            lines.Add($"{name}: reference {assetGuid}\n  <field>:\n    idInternal: 0\n    valueGuidInternal: \n    WwiseObjectReference: {{fileID: 11400000, guid: {assetGuid}, type: 2}}");
        }
        AssetDatabase.SaveAssets();
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Sets an AK.Wwise.Event field of a ScriptableObject asset (not a prefab: edit those in YAML) to an event
    /// </summary>
    public static string Set(string assetPath, string fieldPath, string eventName)
    {
        var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        if (asset == null || asset is GameObject) return "not a ScriptableObject asset: " + assetPath;
        if (!Events().TryGetValue(eventName, out System.Guid guid)) return eventName + " is not in the Wwise project";
        var serialized = new SerializedObject(asset);
        var property = serialized.FindProperty(fieldPath + ".WwiseObjectReference");
        if (property == null) return $"no AK.Wwise.Event field {fieldPath} on {asset.name}";
        property.objectReferenceValue = WwiseObjectReference.FindOrCreateWwiseObject(WwiseObjectType.Event, eventName, guid);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(asset);
        return $"{asset.name}.{fieldPath} = {eventName}";
    }
}
