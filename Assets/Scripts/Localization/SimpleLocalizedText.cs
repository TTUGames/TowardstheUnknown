[System.Serializable]

public class SimpleLocalizedText : LocalizedText
{
    public string TEXT;

	public override void Sanitize() {
		TEXT = Sanitize(TEXT);
	}
}
