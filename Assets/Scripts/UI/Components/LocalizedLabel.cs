using UnityEngine.Localization;
using UnityEngine.UIElements;

/// <summary>
/// A label showing an entry of the UI string table, updated when the language changes
/// </summary>
[UxmlElement]
public partial class LocalizedLabel : Label, ILocalizedText
{
    private string textKey;

    [UxmlAttribute]
    public string key { get => textKey; set => LocalizedText.Bind(this, textKey = value); }
}

/// <summary>
/// Binds the text of an element to an entry of the UI string table
/// </summary>
public static class LocalizedText
{
    public static void Bind(TextElement element, string key)
    {
        if (string.IsNullOrEmpty(key))
            element.ClearBinding("text");
        else
            element.SetBinding("text", new LocalizedString(Localization.UITable, key));
    }
}

/// <summary>
/// An element whose text is an entry of the UI string table
/// </summary>
public interface ILocalizedText
{
    string key { get; set; }
}
