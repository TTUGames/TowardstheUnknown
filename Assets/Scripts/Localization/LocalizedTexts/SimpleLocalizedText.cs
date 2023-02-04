[System.Serializable]

public class SimpleLocalizedText : LocalizedText
{
    public string TEXT;

	public SimpleLocalizedText() {
		TEXT = "Translation not found !";
	}

	public override void Sanitize() {
		TEXT = Sanitize(TEXT);
	}
}
