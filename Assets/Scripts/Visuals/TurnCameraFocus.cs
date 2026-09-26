using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// A short, gentle nudge of the camera when the side changes: when the enemies' turns begin, it eases a little towards the
/// first enemy to act, and back to its rest on the player's turn, at the end of the combat, of the run or when leaving the room.
/// It only computes an offset: <see cref="ImpactFeedback"/>, the one writer of the camera's transform, adds it under its
/// shake. Purely visual, it never waits for nor delays the gameplay
/// </summary>
public class TurnCameraFocus : MonoBehaviour
{
    [SerializeField, Required, Tooltip("Its rest pose, read at the start, tells where the middle of the screen falls on the board")] private Transform focusedCamera;
    [SerializeField, Range(0, 1), Tooltip("Share of the distance from the middle of the screen to the enemy the camera travels")] private float strength = 0.15f;
    [SerializeField, Min(0), SuffixLabel("m"), Tooltip("Longest shift from the rest")] private float maxShift = 0.35f;
    [SerializeField, Min(0.01f), SuffixLabel("s"), Tooltip("Time to ease towards the enemies' side")] private float focusDuration = 0.6f;
    [SerializeField, Min(0.01f), SuffixLabel("s"), Tooltip("Time to ease back to the rest")] private float returnDuration = 0.45f;

    private Vector3 restPosition;
    private Quaternion restRotation;
    private Vector3 from, to;
    private float progress = 1f;
    private float duration = 1f;
    private bool enemySide;

    /// <summary>
    /// The shift from the camera's rest, in its parent's space: zero at rest
    /// </summary>
    public Vector3 Offset { get; private set; }

    private void Awake()
    {
        restPosition = focusedCamera.position;
        restRotation = focusedCamera.rotation;
    }

    private void OnEnable()
    {
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
        GameEvents.RunEnded += OnRunEnded;
        GameEvents.RoomLeft += Snap;
    }

    private void OnDisable()
    {
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
        GameEvents.RunEnded -= OnRunEnded;
        GameEvents.RoomLeft -= Snap;
        Snap();
    }

    // Before ImpactFeedback's LateUpdate. In scaled time: it holds through the pause and the hit stops
    private void Update()
    {
        if (progress >= 1f) return;
        progress = Mathf.Min(1f, progress + Time.deltaTime / duration);
        Offset = progress >= 1f ? to : Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, progress));
    }

    private void OnTurnChanged()
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        EntityTurn current = turnSystem.Current;
        if (current == null || turnSystem.IsPlayerTurn)
        {
            enemySide = false;
            MoveTo(Vector3.zero, returnDuration);
            return;
        }
        // Once per side, on the first enemy to act: not at each enemy's turn
        if (enemySide) return;
        enemySide = true;
        MoveTo(ShiftTowards(current.transform.position), focusDuration);
    }

    private void OnRunEnded(bool isVictory)
    {
        enemySide = false;
        MoveTo(Vector3.zero, returnDuration);
    }

    private void Snap()
    {
        enemySide = false;
        from = to = Offset = Vector3.zero;
        progress = 1f;
    }

    private void MoveTo(Vector3 target, float seconds)
    {
        if (target == to) return; //Already there or on its way
        from = Offset;
        to = target;
        duration = seconds;
        progress = 0f;
    }

    // Along the ground, from the point the middle of the screen shows at the entity's height towards the entity
    private Vector3 ShiftTowards(Vector3 entity)
    {
        Vector3 forward = restRotation * Vector3.forward;
        if (Mathf.Abs(forward.y) < 0.01f) return Vector3.zero;
        Vector3 center = restPosition + forward * ((entity.y - restPosition.y) / forward.y);
        Vector3 toEntity = entity - center;
        toEntity.y = 0f;
        Vector3 shift = Vector3.ClampMagnitude(toEntity * strength, maxShift);
        Transform parent = focusedCamera.parent;
        return parent != null ? parent.InverseTransformVector(shift) : shift;
    }
}
