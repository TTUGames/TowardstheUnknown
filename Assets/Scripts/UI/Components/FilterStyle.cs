using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// A filter style set many times a second (the animated filters of <see cref="ArtifactPiece"/> and <see cref="HealthBar"/>)
/// without allocating: its element keeps two lists and alternates them, since UI Toolkit compares a style list by reference
/// (the same list changed would not be redrawn)
/// </summary>
public static class FilterStyle
{
    /// <summary>
    /// The two lists an element alternates
    /// </summary>
    public static List<FilterFunction>[] Lists() => new[] { new List<FilterFunction>(1), new List<FilterFunction>(1) };

    /// <summary>
    /// The style of a single filter function, in the list not used last
    /// </summary>
    public static StyleList<FilterFunction> Next(List<FilterFunction>[] lists, ref int index, FilterFunction function)
    {
        index ^= 1;
        List<FilterFunction> list = lists[index];
        list.Clear();
        list.Add(function);
        return new StyleList<FilterFunction>(list);
    }
}
