/// <summary>
/// Basic class for all of the localized descriptions
/// </summary>
[System.Serializable]
public abstract class LocalizedText
{
    private const string highlightColor = "#e82a65";

    public string ID;

    /// <summary>
    /// Replaces all the abstract tags in all this LocalizedText's strings
    /// </summary>
    public virtual void Sanitize() { }

    /// <summary>
    /// Replaces the damage (D) and block (B) tags in the text's strings with colors
    /// </summary>
    /// <param name="s">The original string</param>
    /// <returns>The string with sanitized tags</returns>
    protected string Sanitize(string s) {
        return s?.Replace("<D>", "<color=" + highlightColor + ">").Replace("</D>", "</color>")
                 .Replace("<B>", "<color=" + highlightColor + ">").Replace("</B>", "</color>");
    }
}
