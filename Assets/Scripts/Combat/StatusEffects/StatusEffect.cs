/// <summary>
/// Runtime instance of a <c>StatusEffectData</c> on an entity, holding its remaining turns
/// </summary>
public class StatusEffect
{
    public StatusEffect(StatusEffectData data, int duration) {
        Data = data;
        Duration = duration;
    }

    public StatusEffectData Data { get; }
    public int Duration { get; set; }

    /// <summary>
    /// Put on the entity by itself: counted down at the start of its turns rather than at their end
    /// </summary>
    public bool SelfApplied { get; set; }
}
