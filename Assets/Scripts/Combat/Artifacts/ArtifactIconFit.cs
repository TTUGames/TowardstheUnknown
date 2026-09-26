/// <summary>
/// How an artifact's icon is laid on its piece in the inventory, in the largest rectangle of its shape
/// </summary>
public enum ArtifactIconFit
{
    /// <summary>Whole, keeping its proportions</summary>
    Contain,
    /// <summary>Covering the whole rectangle, keeping its proportions: larger, cut by the shape</summary>
    Fill,
}
