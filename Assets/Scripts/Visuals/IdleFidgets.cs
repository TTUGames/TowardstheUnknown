using UnityEngine;

/// <summary>
/// Plays now and then one of the entity's idle variations (<see cref="EntityData.idleVariations"/>) while it waits for the
/// combat, through the deployment: over its idle as an attack is, cut by a move, never once the combat started.
/// Removed from the Classic by the prefab's EditionOnly
/// </summary>
[RequireComponent(typeof(EntityAnimator))]
public class IdleFidgets : MonoBehaviour
{
    [SerializeField, Tooltip("Seconds of plain idle between two variations, drawn between x and y")] private Vector2 interval = new Vector2(5, 12);

    private EntityStats stats;
    private EntityAnimator entityAnimator;
    private float next;
    private int last = -1;

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
        entityAnimator = GetComponent<EntityAnimator>();
    }

    //Each entity on its own clock, so that a room's pack doesn't move as one
    private void OnEnable() => next = Time.time + Random.Range(0, interval.y);

    private void Update()
    {
        if (Time.time < next) return;
        next = Time.time + Random.Range(interval.x, interval.y);
        AnimationClip[] clips = stats.Data != null ? stats.Data.idleVariations : null;
        if (clips == null || clips.Length == 0 || TurnSystem.Instance.IsCombat || stats.IsDead || entityAnimator.IsAttacking) return;
        //Never the same twice in a row
        int index = Random.Range(0, clips.Length - (last >= 0 && clips.Length > 1 ? 1 : 0));
        if (last >= 0 && clips.Length > 1 && index >= last) index++;
        last = index;
        entityAnimator.PlayAttack(clips[index]);
        next += clips[index].length;
        //Nothing to wait for: a move cuts it at once
        entityAnimator.EndAttack();
    }
}
