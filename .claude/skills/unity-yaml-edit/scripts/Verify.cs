using System.Linq;
using UnityEditor;
using UnityEngine;

// Checks a prefab or asset after a YAML edit, run through: unity --json command run_script --file <this file> --entry Verify.<Method> --args '[...]'

public static class Verify
{
    /// <summary>
    /// Reimports the file, then lists the components of each GameObject of a prefab and its missing scripts
    /// </summary>
    public static string Prefab(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (root == null) return "not a prefab: " + path;
        int missing = root.GetComponentsInChildren<Component>(true).Count(c => c == null);
        var withScripts = root.GetComponentsInChildren<Transform>(true)
            .Select(t => t.name + ": " + string.Join(", ", t.GetComponents<MonoBehaviour>().Select(c => c == null ? "MISSING" : c.GetType().Name)))
            .Where(line => !line.EndsWith(": "));
        return $"missing scripts: {missing}\n" + string.Join("\n", withScripts);
    }

    /// <summary>
    /// Prints a serialized property of a component of a prefab (Type, property path such as "sound.WwiseObjectReference" or "list.Array.data[0]")
    /// </summary>
    public static string Field(string path, string componentType, string propertyPath)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Object target = root != null
            ? root.GetComponentsInChildren<Component>(true).FirstOrDefault(c => c != null && c.GetType().Name == componentType)
            : AssetDatabase.LoadMainAssetAtPath(path);
        if (target == null) return $"no {componentType} in {path}";
        var property = new SerializedObject(target).FindProperty(propertyPath);
        if (property == null) return $"no property {propertyPath} on {target.GetType().Name}";
        return property.propertyType switch
        {
            SerializedPropertyType.ObjectReference => $"{propertyPath} = {(property.objectReferenceValue != null ? property.objectReferenceValue.name + " (" + AssetDatabase.GetAssetPath(property.objectReferenceValue) + ")" : "null")}",
            SerializedPropertyType.Integer => $"{propertyPath} = {property.intValue}",
            SerializedPropertyType.Float => $"{propertyPath} = {property.floatValue}",
            SerializedPropertyType.Boolean => $"{propertyPath} = {property.boolValue}",
            SerializedPropertyType.String => $"{propertyPath} = \"{property.stringValue}\"",
            SerializedPropertyType.Enum => $"{propertyPath} = {property.enumNames.ElementAtOrDefault(property.enumValueIndex)}",
            _ when property.isArray => $"{propertyPath} = {property.arraySize} elements",
            _ => $"{propertyPath} ({property.propertyType})",
        };
    }
}
