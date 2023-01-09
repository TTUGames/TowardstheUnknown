using System.Text.RegularExpressions;

[System.Serializable]

public class ArtifactDescription : LocalizedText
{
	private static string damageColor = "#e82a65";


	public string TITLE;
    public string DESCRIPTION;
    public string EFFECTS;

	public override void Sanitize() {
		EFFECTS = Regex.Replace(EFFECTS, "<D>", "<color=" + damageColor + ">");
		EFFECTS = Regex.Replace(EFFECTS, "</D>", "</color>");
	}
}
