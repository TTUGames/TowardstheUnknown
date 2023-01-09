using System.Text.RegularExpressions;

[System.Serializable]

public class ArtifactDescription : LocalizedText
{


	public string TITLE;
    public string DESCRIPTION;
    public string EFFECTS;

	public override void Sanitize() {
		TITLE = Sanitize(TITLE);
		DESCRIPTION = Sanitize(DESCRIPTION);
		EFFECTS = Sanitize(EFFECTS);
	}
}
