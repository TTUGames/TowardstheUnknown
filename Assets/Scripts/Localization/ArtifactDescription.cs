using System.Text.RegularExpressions;

[System.Serializable]

public class ArtifactDescription : LocalizedText
{


	public string TITLE;
    public string DESCRIPTION;
    public string EFFECTS;

	public ArtifactDescription() {
		DESCRIPTION = "Translation not found !";
	}

	public override void Sanitize() {
		TITLE = Sanitize(TITLE);
		DESCRIPTION = Sanitize(DESCRIPTION);
		EFFECTS = Sanitize(EFFECTS);
	}
}
