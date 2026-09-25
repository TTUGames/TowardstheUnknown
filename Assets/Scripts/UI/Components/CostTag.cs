using UnityEngine.UIElements;

/// <summary>
/// The parallelogram tag showing an energy cost
/// </summary>
[UxmlElement]
public partial class CostTag : SlantedPanel
{
    private readonly Label label = new();

    public CostTag()
    {
        corners = Corners.TopLeft | Corners.BottomRight;
        AddToClassList("cost-tag");
        label.pickingMode = PickingMode.Ignore;
        label.AddToClassList("cost-tag__value");
        Add(label);
    }

    [UxmlAttribute]
    public int value { get => int.TryParse(label.text, out int cost) ? cost : 0; set => label.text = value.ToString(); }
}
