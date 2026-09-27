using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
// Reimports the styles and makes the live panels restyle: they keep the old rules of an edited sheet until their theme is set again
public static class ReloadStyles { public static string Run() {
  foreach (var path in AssetDatabase.FindAssets("t:StyleSheet", new[] { "Assets/UI" })) AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(path), ImportAssetOptions.ForceUpdate);
  int n = 0;
  foreach (var d in Object.FindObjectsByType<UIDocument>()) { var ps = d.panelSettings; var t = ps.themeStyleSheet; ps.themeStyleSheet = null; ps.themeStyleSheet = t; n++; }
  return "reloaded, panels " + n; } }
