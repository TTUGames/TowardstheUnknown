using System.Text.RegularExpressions;

[System.Serializable]

public class ArtifactDescription : LocalizedText
{


	public string TITLE;
    public string DESCRIPTION;
    public string EFFECTS;
    public string RANGE;
    public string COOLDOWN;

	public ArtifactDescription() {
		DESCRIPTION = "Translation not found !";
	}

	public override void Sanitize() {
		TITLE = Sanitize(TITLE);
		DESCRIPTION = Sanitize(DESCRIPTION);
		EFFECTS = Sanitize(EFFECTS);
		RANGE = Sanitize(RANGE);
		COOLDOWN = Sanitize(COOLDOWN);
	}
}
