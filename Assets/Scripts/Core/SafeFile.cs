using System;
using System.IO;
using UnityEngine;

/// <summary>
/// The game's files (settings, run save, command journal) are replaced at once: a crash while writing keeps the previous one
/// </summary>
public static class SafeFile
{
    /// <summary>
    /// Writes the text to the file through a temporary one; false, with a warning naming <paramref name="what"/>, if it failed
    /// </summary>
    public static bool Write(string path, string text, string what)
    {
        try
        {
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, text);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temporary, path);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(what + " not written: " + exception.Message);
            return false;
        }
    }
}
