using System.Text.RegularExpressions;

/// <summary>
/// Basic class for all of the localized descriptions
/// </summary>
public abstract class LocalizedText
{
    private static string damageColor = "#e82a65";
    private static string blockColor = "#e82a65";

    public string ID;

    /// <summary>
    /// Raplces all the abstract tags in all this LocalizedText's strings
    /// </summary>
    public virtual void Sanitize() { }

    /// <summary>
    /// Replaces all the abstract tags in the text's strings
    /// </summary>
    /// <param name="s">The original string</param>
    /// <returns>The string with sanitized tags</returns>
    protected string Sanitize(string s) {
        string sanitized = s;
        sanitized = Regex.Replace(sanitized, "<D>", "<color=" + damageColor + ">");
        sanitized = Regex.Replace(sanitized, "</D>", "</color>");
        sanitized = Regex.Replace(sanitized, "<B>", "<color=" + blockColor + ">");
        sanitized = Regex.Replace(sanitized, "</B>", "</color>");

        return sanitized;
    }
}
