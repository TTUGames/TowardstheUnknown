using UnityEngine.UIElements;

/// <summary>
/// The big uppercase text button of the menus. Hovering it colors it and draws an accent bar under its text.
/// Its text is a child label: an element with children is not sized by its own text, so the label gives the button its size
/// </summary>
[UxmlElement]
public partial class MenuButton : Button, ILocalizedText
{
    private readonly Label label;
    private string textKey;

    public MenuButton()
    {
        AddToClassList("flat-button");
        AddToClassList("menu-button");
        label = new Label { pickingMode = PickingMode.Ignore };
        label.AddToClassList("menu-button__text");
        label.AddToClassList(MenuScreen.CapsClassName);
        Add(label);
        var bar = new VisualElement { pickingMode = PickingMode.Ignore };
        bar.AddToClassList("menu-button__bar");
        Add(bar);
    }

    /// <summary>
    /// The entry of the UI string table shown on the button
    /// </summary>
    [UxmlAttribute]
    public string key { get => textKey; set => LocalizedText.Bind(label, textKey = value); }
}
