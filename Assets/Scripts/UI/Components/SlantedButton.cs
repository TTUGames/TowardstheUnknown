using UnityEngine.UIElements;

/// <summary>
/// A button drawn with the game's slanted shape (CutShape), its text read from the UI string table
/// </summary>
[UxmlElement]
public partial class SlantedButton : Button, ILocalizedText
{
    private readonly CutShape shape;
    private string textKey;

    public SlantedButton()
    {
        shape = new CutShape(this);
        AddToClassList("flat-button");
    }

    [UxmlAttribute]
    public Corners corners { get => shape.Corners; set => shape.Corners = value; }

    [UxmlAttribute]
    public Corners dots { get => shape.Dots; set => shape.Dots = value; }

    /// <summary>
    /// The entry of the UI string table shown on the button
    /// </summary>
    [UxmlAttribute]
    public string key { get => textKey; set => LocalizedText.Bind(this, textKey = value); }
}
