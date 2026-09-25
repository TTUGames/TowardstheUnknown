using UnityEngine.UIElements;

/// <summary>
/// The big uppercase text button of the menus. Hovering it colors it and draws an accent bar under its text
/// </summary>
[UxmlElement]
public partial class MenuButton : Button
{
    private string textKey;

    public MenuButton()
    {
        AddToClassList("flat-button");
        AddToClassList("menu-button");
        AddToClassList(MenuScreen.CapsClassName);
        var bar = new VisualElement { pickingMode = PickingMode.Ignore };
        bar.AddToClassList("menu-button__bar");
        Add(bar);
    }

    /// <summary>
    /// The entry of the UI string table shown on the button
    /// </summary>
    [UxmlAttribute]
    public string key { get => textKey; set => LocalizedText.Bind(this, textKey = value); }
}
