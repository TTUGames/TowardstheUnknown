using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// The player's settings and progress (options, edition, language, bindings, best score, hints seen) in a JSON file of the
/// game's data folder, <c>settings.json</c>, rather than in the PlayerPrefs, which live in the registry on Windows, where Steam
/// Auto-Cloud can't reach them. The same calls as the PlayerPrefs; <see cref="Save"/> writes the file, as does quitting.
/// On first use without a file, the keys saved in the PlayerPrefs by earlier versions are taken over. The editor keeps its own
/// file, as it kept its own PlayerPrefs
/// </summary>
public static class Prefs
{
    [Serializable]
    private class Data
    {
        public List<Entry> ints = new List<Entry>(), floats = new List<Entry>(), strings = new List<Entry>();
    }

    [Serializable]
    private class Entry
    {
        public string key, value;
    }

    // The keys earlier versions saved in the PlayerPrefs, taken over once; the hints seen are HintPrefix + their id
    private static readonly string[] IntKeys = { "Edition", "BestScore", "DisclaimerSeen", "FrameRateCap" };
    private static readonly string[] StringKeys = { "Bindings", "Language", "Resolution" };
    private static readonly string[] HintIds = { "Deploy", "Turn", "EndTurn", "Chest" };
    public const string HintPrefix = "HintSeen.";

    private static readonly Dictionary<string, int> ints = new();
    private static readonly Dictionary<string, float> floats = new();
    private static readonly Dictionary<string, string> strings = new();
    private static bool loaded, dirty;

    private static string FilePath => Path.Combine(Application.persistentDataPath, Application.isEditor ? "settings-editor.json" : "settings.json");

    public static int GetInt(string key, int fallback = 0) => Loaded(ints).TryGetValue(key, out int value) ? value : fallback;
    public static float GetFloat(string key, float fallback = 0) => Loaded(floats).TryGetValue(key, out float value) ? value : fallback;
    public static string GetString(string key, string fallback = "") => Loaded(strings).TryGetValue(key, out string value) ? value : fallback;
    public static bool HasKey(string key) => Loaded(ints).ContainsKey(key) || floats.ContainsKey(key) || strings.ContainsKey(key);

    public static void SetInt(string key, int value) => Set(ints, key, value);
    public static void SetFloat(string key, float value) => Set(floats, key, value);
    public static void SetString(string key, string value) => Set(strings, key, value);

    public static void DeleteKey(string key)
    {
        Load();
        dirty |= ints.Remove(key) | floats.Remove(key) | strings.Remove(key);
    }

    /// <summary>
    /// Writes the file if something changed since it was last written
    /// </summary>
    public static void Save()
    {
        if (!loaded || !dirty) return;
        var data = new Data();
        foreach (var (key, value) in ints) data.ints.Add(new Entry { key = key, value = value.ToString(System.Globalization.CultureInfo.InvariantCulture) });
        foreach (var (key, value) in floats) data.floats.Add(new Entry { key = key, value = value.ToString("R", System.Globalization.CultureInfo.InvariantCulture) });
        foreach (var (key, value) in strings) data.strings.Add(new Entry { key = key, value = value });
        try
        {
            string temporary = FilePath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
            // Replaced at once: a crash while writing keeps the previous file
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(temporary, FilePath);
            dirty = false;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Settings not written: " + exception.Message);
        }
    }

    private static void Set<T>(Dictionary<string, T> values, string key, T value)
    {
        Load();
        if (values.TryGetValue(key, out T current) && EqualityComparer<T>.Default.Equals(current, value)) return;
        values[key] = value;
        dirty = true;
    }

    private static Dictionary<string, T> Loaded<T>(Dictionary<string, T> values)
    {
        Load();
        return values;
    }

    // Once per launch (the editor's Play mode keeps it: the file is the truth outside Play mode too)
    private static void Load()
    {
        if (loaded) return;
        loaded = true;
        Application.quitting -= Save;
        Application.quitting += Save;
        if (!File.Exists(FilePath))
        {
            ImportPlayerPrefs();
            return;
        }
        try
        {
            Data data = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            foreach (Entry entry in data.ints)
                if (int.TryParse(entry.value, System.Globalization.NumberStyles.Integer, culture, out int value)) ints[entry.key] = value;
            foreach (Entry entry in data.floats)
                if (float.TryParse(entry.value, System.Globalization.NumberStyles.Float, culture, out float value)) floats[entry.key] = value;
            foreach (Entry entry in data.strings) strings[entry.key] = entry.value ?? "";
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Settings unreadable, back to the defaults: " + exception.Message);
        }
    }

    // The settings of the versions before the file: every other GameSetting is a float (under its old name for the first ones)
    private static void ImportPlayerPrefs()
    {
        foreach (string key in IntKeys) Import(key, PlayerPrefs.GetInt, ints);
        foreach (string id in HintIds) Import(HintPrefix + id, PlayerPrefs.GetInt, ints);
        foreach (string key in StringKeys) Import(key, PlayerPrefs.GetString, strings);
        foreach (GameSetting setting in Enum.GetValues(typeof(GameSetting)))
            // Saved apart, as "Resolution" and "FrameRateCap" above
            if (setting is not (GameSetting.Resolution or GameSetting.FrameRateCap)) Import(GameSettings.Key(setting), PlayerPrefs.GetFloat, floats);
        // Written at once, so that the next launch reads the file even if nothing changes
        dirty = true;
        Save();
    }

    private static void Import<T>(string key, Func<string, T> read, Dictionary<string, T> values)
    {
        // A key saved as another type reads as the fallback: HasKey tells it was saved
        if (PlayerPrefs.HasKey(key)) values[key] = read(key);
    }
}
