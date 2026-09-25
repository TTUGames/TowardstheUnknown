using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// A row of energy cells: the full ones, the ones an action would cost and the spent ones
/// </summary>
[UxmlElement]
public partial class EnergyGauge : VisualElement
{
    private readonly List<SlantedPanel> cells = new();

    public EnergyGauge()
    {
        AddToClassList("energy");
    }

    public void Set(int current, int max, int previewed = 0)
    {
        while (cells.Count < max)
        {
            var cell = new SlantedPanel(Corners.TopLeft | Corners.BottomRight, "energy-cell") { pickingMode = PickingMode.Ignore };
            cells.Add(cell);
            Add(cell);
        }
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].EnableInClassList("hidden", i >= max);
            cells[i].EnableInClassList("energy-cell--empty", i >= current);
            cells[i].EnableInClassList("energy-cell--previewed", i < current && i >= current - previewed);
        }
    }
}
