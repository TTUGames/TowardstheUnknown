using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// A suspended run, written as the player goes from a room to the next (<see cref="Map"/>), in an edition with
/// <see cref="EditionProfile.suspendRun"/>: the map's seed, the rooms visited, the room entered and the side it was entered
/// from, the player's health and grid, the run's progress. The main menu's Continue loads it (<see cref="GameFlow.ContinueRun"/>):
/// the run goes on as the player entered that room. A new run or the run's end deletes it
/// </summary>
public static class RunSave
{
    [Serializable]
    public class Data
    {
        public int version = Version;
        public int seed;
        public Vector2Int room;
        public Direction enteredFrom;
        public List<Vector2Int> visitedRooms = new();
        public int health;
        public List<Item> items = new();
        public string playerName;
        public int score;
        public int visitedRoomCount;
        public List<string> killFamilies = new();
        public List<int> killCounts = new();
    }

    /// <summary>
    /// An artifact of the player's grid: its ID, slot and rotation, in the order of the skills
    /// </summary>
    [Serializable]
    public class Item
    {
        public string artifact;
        public Vector2Int slot;
        public int rotation;
    }

    // A save of another version is dropped
    private const int Version = 1;
    private static string FilePath => Path.Combine(Application.persistentDataPath, "run.json");

    /// <summary>
    /// The save the run being loaded goes on from, null for a new run
    /// </summary>
    public static Data Resumed { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        Resumed = null;
    }

    public static bool Exists => File.Exists(FilePath);

    /// <summary>
    /// Reads the save for the run about to load: false if there is none or it can't be read (then deleted)
    /// </summary>
    public static bool Resume()
    {
        Resumed = null;
        if (!Exists) return false;
        try
        {
            Data data = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));
            if (data != null && data.version == Version) Resumed = data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Run save unreadable, deleted: " + exception.Message);
        }
        if (Resumed == null) Delete();
        return Resumed != null;
    }

    /// <summary>
    /// The next run is a new one
    /// </summary>
    public static void StartNew()
    {
        Resumed = null;
        Delete();
    }

    public static void Write(Data data)
    {
        try
        {
            string temporary = FilePath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(data));
            // Replaced at once: a crash while writing keeps the previous save
            if (File.Exists(FilePath)) File.Delete(FilePath);
            File.Move(temporary, FilePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Run save not written: " + exception.Message);
        }
    }

    public static void Delete()
    {
        try
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Run save not deleted: " + exception.Message);
        }
    }
}
