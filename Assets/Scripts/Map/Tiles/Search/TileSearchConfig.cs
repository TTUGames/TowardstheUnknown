using Sirenix.OdinInspector;

/// <summary>
/// Serializable description of a <c>TileSearch</c>, instantiated at runtime
/// </summary>
[System.Serializable, InlineProperty]
public class TileSearchConfig
{
    public enum Shape
    {
        [LabelText("Circle")] Circle,
        [LabelText("Circle, line of sight")] CircleAttack,
        [LabelText("Circle, walkable path")] CircleWalkable,
        [LabelText("Line")] Line,
        [LabelText("Line, line of sight")] LineAttack,
        [LabelText("Line, free path (rush)")] Rush,
    }

    [HorizontalGroup(0.5f), HideLabel] public Shape shape = Shape.CircleAttack;
    [HorizontalGroup, LabelWidth(30), MinValue(0)] public int min;
    [HorizontalGroup, LabelWidth(30), MinValue("min")] public int max;

    public TileSearchConfig() { }

    public TileSearchConfig(Shape shape, int min, int max)
    {
        this.shape = shape;
        this.min = min;
        this.max = max;
    }

    public TileSearch Create() => shape switch
    {
        Shape.Circle => new CircleTileSearch(min, max),
        Shape.CircleAttack => new CircleTileSearch(min, max).Selecting(TileConstraints.LineOfSight),
        Shape.CircleWalkable => new CircleTileSearch(min, max).Through(TileConstraints.Walkable),
        Shape.Line => new LineTileSearch(min, max),
        Shape.LineAttack => new LineTileSearch(min, max).Selecting(TileConstraints.LineOfSight),
        _ => new LineTileSearch(min, max).Through(TileConstraints.Walkable, TileConstraints.Empty),
    };
}
