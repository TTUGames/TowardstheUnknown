using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// Builds the Addressables content (the ClassicSkin group) with the active builder: the player build does not
// (AddressableAssetSettings.BuildAddressablesWithPlayerBuild is off). Run through script run before `build run`.
public static class BuildAddressables
{
    public static string Run()
    {
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent(out var result);
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError("Addressables build failed: " + result.Error);
            return "FAILED " + result.Error;
        }
        return $"ok {result.Duration:F1}s";
    }
}
