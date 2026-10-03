using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
// Reimports the edited style sheets and makes the live panels restyle: they keep the old rules of an edited sheet until their
// theme is set again. Only the sheets passed (comma-separated, those written since Play mode started or the last call): reimporting every sheet of Assets/UI
// in Play mode leaves the panels without their texts
public static class ReloadStyles { public static string Run(string paths) {
  int sheets = 0;
  foreach (var path in paths.Split(','))
    if (path.EndsWith(".uss") || path.EndsWith(".tss")) { AssetDatabase.ImportAsset(path.Trim(), ImportAssetOptions.ForceUpdate); sheets++; }
  int n = 0;
  foreach (var d in Object.FindObjectsByType<UIDocument>()) { var ps = d.panelSettings; var t = ps.themeStyleSheet; ps.themeStyleSheet = null; ps.themeStyleSheet = t; n++; }
  return $"reimported {sheets} sheets, restyled panels {n}"; } }
