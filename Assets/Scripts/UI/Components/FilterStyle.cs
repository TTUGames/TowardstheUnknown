using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The animated filter of an element (<see cref="ArtifactPiece"/>, the parts of a <see cref="HealthBar"/>), set many times a
/// second without allocating: it keeps two lists and alternates them, since UI Toolkit compares a style list by reference
/// (the same list changed would not be redrawn)
/// </summary>
public class FilterStyle
{
    private readonly VisualElement element;
    private readonly List<FilterFunction>[] lists = { new(1), new(1) };
    private int index;

    public FilterStyle(VisualElement element) => this.element = element;

    /// <summary>
    /// Draws the element through the filter, given its three parameters then the time: each call redraws it
    /// </summary>
    public void Animate(FilterFunctionDefinition effect, FilterParameter first, FilterParameter second, FilterParameter third)
    {
        var function = new FilterFunction(effect);
        function.AddParameter(first);
        function.AddParameter(second);
        function.AddParameter(third);
        function.AddParameter(new FilterParameter(Time.unscaledTime % 1000));
        index ^= 1;
        List<FilterFunction> list = lists[index];
        list.Clear();
        list.Add(function);
        element.style.filter = new StyleList<FilterFunction>(list);
    }

    /// <summary>
    /// Leaves the filter out (the Classic edition's style)
    /// </summary>
    public void Clear()
    {
        if (element.style.filter.keyword != StyleKeyword.Null) element.style.filter = StyleKeyword.Null;
    }
}
