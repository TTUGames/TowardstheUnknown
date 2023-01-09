using System.Text.RegularExpressions;

/// <summary>
/// Basic class for all of the localized descriptions
/// </summary>
public abstract class LocalizedText
{
    public string ID;

    public virtual void Sanitize() { }
}
