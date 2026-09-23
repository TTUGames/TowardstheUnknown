using System.Collections.Generic;
using UnityEngine;

public static class ListShuffler
{
    /// <summary>
    /// Shuffles the list in place (Fisher-Yates)
    /// </summary>
    public static void Shuffle<T>(List<T> list) {
        for (int i = list.Count - 1; i > 0; --i) {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
	}
}
