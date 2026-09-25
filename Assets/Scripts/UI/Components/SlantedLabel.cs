using UnityEngine.UIElements;

/// <summary>
/// A text drawn in the game's slanted shape (CutShape): tooltips and small popups
/// </summary>
[UxmlElement]
public partial class SlantedLabel : Label
{
    private readonly CutShape shape;

    public SlantedLabel()
    {
        shape = new CutShape(this);
    }

    [UxmlAttribute]
    public Corners corners { get => shape.Corners; set => shape.Corners = value; }

    [UxmlAttribute]
    public Corners dots { get => shape.Dots; set => shape.Dots = value; }
}
