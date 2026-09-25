/// <summary>
/// Something that takes time or must happen in order, processed by the <c>ActionManager</c> queue
/// </summary>
public abstract class GameAction
{
    public bool isDone = false;
    public bool IsStarted { get; private set; }

    /// <summary>
    /// Called by the <c>ActionManager</c> once, when the action reaches the head of the queue
    /// </summary>
    public void Start() {
        IsStarted = true;
        OnStart();
    }

    /// <summary>
    /// Starts the action. Use it rather than the constructor for anything visible or timed.
    /// </summary>
    protected virtual void OnStart() { }

    /// <summary>
    /// Called every frame while the action is at the head of the queue, until <c>isDone</c> is set
    /// </summary>
    public virtual void Apply() { }
}
