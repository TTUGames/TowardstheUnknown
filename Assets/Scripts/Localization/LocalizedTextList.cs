using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// List of localized descriptions
/// </summary>
/// <typeparam name="T"></typeparam>

[System.Serializable]
public class LocalizedTextList<T> where T : LocalizedText
{
    public List<T> content;

    /// <summary>
    /// Gets a dictionnary with IDs as keys from the description list
    /// </summary>
    /// <returns>The descriptions stored as a dictionary</returns>
    public Dictionary<string, T> ToDictionary() {
        Dictionary<string, T> dictionary = new Dictionary<string, T>();
        foreach(T localizedText in content) {
            localizedText.Sanitize();
            dictionary.Add(localizedText.ID, localizedText);
		}
        return dictionary;
	}
}
