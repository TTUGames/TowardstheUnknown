using UnityEngine;

/// <summary>
/// Turns an entity, level, towards a point at a speed in degrees per second, until it faces it
/// </summary>
public class TurnTowardsAction : GameAction
{
    private readonly Transform turned;
    private readonly Vector3 point;
    private readonly float speed;

    public TurnTowardsAction(Transform turned, Vector3 point, float speed)
    {
        this.turned = turned;
        this.point = point;
        this.speed = speed;
    }

    public override void Apply()
    {
        if (turned == null)
        {
            isDone = true;
            return;
        }
        Vector3 towards = point - turned.position;
        towards.y = 0;
        if (towards.sqrMagnitude < 1e-4f)
        {
            isDone = true;
            return;
        }
        Quaternion facing = Quaternion.LookRotation(towards);
        turned.rotation = Quaternion.RotateTowards(turned.rotation, facing, speed * Time.deltaTime);
        isDone = Quaternion.Angle(turned.rotation, facing) < 0.5f;
    }
}
