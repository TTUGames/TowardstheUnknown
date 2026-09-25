using UnityEngine.UIElements;

/// <summary>
/// An element drawn with the game's slanted shape (CutShape): panels, bars, tags. Its look comes from USS
/// (the panel class gives the frosted panel look)
/// </summary>
[UxmlElement]
public partial class SlantedPanel : VisualElement
{
    private readonly CutShape shape;

    public SlantedPanel()
    {
        shape = new CutShape(this);
    }

    public SlantedPanel(Corners corners, params string[] classes) : this()
    {
        this.corners = corners;
        foreach (string className in classes) AddToClassList(className);
    }

    [UxmlAttribute]
    public Corners corners { get => shape.Corners; set => shape.Corners = value; }

    /// <summary>
    /// The top corners ending the top line with a dot, next to dashes
    /// </summary>
    [UxmlAttribute]
    public Corners dots { get => shape.Dots; set => shape.Dots = value; }
}
