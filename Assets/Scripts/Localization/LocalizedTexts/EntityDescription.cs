[System.Serializable]

public class EntityDescription : LocalizedText
{
    public string NAME;

	public EntityDescription() {
		NAME = "Translation not found !";
	}

	public override void Sanitize() {
		NAME = Sanitize(NAME);
	}
}
