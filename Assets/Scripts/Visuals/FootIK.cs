using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Plants a humanoid's feet on the ground under them: each foot the animation keeps low is laid flat on the ground's
/// surface (a raycast), sunk by <c>sink</c>, and the pelvis goes down so that the lower foot reaches it.
/// A foot the animation lifts (a step, a jump) blends back to its animated pose. Needs the IK pass of the animator's layers.
/// </summary>
[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
    [SerializeField, Tooltip("The colliders the feet stand on")] private LayerMask ground;
    [SerializeField, Range(0, 1)] private float weight = 1;
    [SerializeField, SuffixLabel("m"), Tooltip("How far the soles go into the ground")] private float sink = 0.005f;

    [BoxGroup("Planted feet"), SerializeField, SuffixLabel("m"), Tooltip("Height of the animated sole above the character's feet up to which the foot is planted")]
    private float plantedHeight = 0.03f;
    [BoxGroup("Planted feet"), SerializeField, SuffixLabel("m"), Tooltip("Height of the animated sole from which the foot is free, as animated")]
    private float liftedHeight = 0.1f;
    [BoxGroup("Planted feet"), SerializeField, Range(0, 1), Tooltip("How much of the ground's slope the soles follow")]
    private float slopeFollow = 1;

    [BoxGroup("Pelvis"), SerializeField, SuffixLabel("m"), Tooltip("The most the pelvis moves to reach the ground")] private float maxPelvisOffset = 0.06f;
    [BoxGroup("Pelvis"), SerializeField, SuffixLabel("s"), Tooltip("Time the pelvis takes to follow")] private float pelvisSmoothing = 0.08f;

    [BoxGroup("Raycasts"), SerializeField, SuffixLabel("m"), Tooltip("From how high above the foot the ground is searched")] private float rayAbove = 0.3f;
    [BoxGroup("Raycasts"), SerializeField, SuffixLabel("m"), Tooltip("How far below the foot the ground is searched")] private float rayBelow = 0.3f;

    private Animator animator;
    private float pelvisOffset;
    private float pelvisVelocity;
    private int pelvisFrame = -1;

    /// <summary>
    /// A foot's state in the last IK pass, for the gizmos and the playtest probes
    /// </summary>
    public struct Foot
    {
        public bool grounded;
        public float weight;
        public Vector3 animated, target, groundPoint, groundNormal;
    }

    public Foot Left { get; private set; }
    public Foot Right { get; private set; }
    public float PelvisOffset => pelvisOffset;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (!animator.isHuman) enabled = false;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!enabled || weight <= 0) return;
        float scale = transform.lossyScale.y;
        Foot left = Solve(AvatarIKGoal.LeftFoot, animator.leftFeetBottomHeight * scale);
        Foot right = Solve(AvatarIKGoal.RightFoot, animator.rightFeetBottomHeight * scale);
        Left = left;
        Right = right;

        //The pelvis follows the foot that must go down the most, once per frame whatever the number of IK passes
        if (pelvisFrame != Time.frameCount)
        {
            pelvisFrame = Time.frameCount;
            float wanted = Mathf.Min(Delta(left), Delta(right), 0);
            wanted = Mathf.Max(wanted, -maxPelvisOffset);
            pelvisOffset = Mathf.SmoothDamp(pelvisOffset, wanted, ref pelvisVelocity, pelvisSmoothing);
        }
        animator.bodyPosition += Vector3.up * (pelvisOffset * weight);

        Apply(AvatarIKGoal.LeftFoot, left);
        Apply(AvatarIKGoal.RightFoot, right);
    }

    // How far the foot's target is below its animated position, weighted by how planted it is
    private static float Delta(Foot foot) => foot.grounded ? (foot.target.y - foot.animated.y) * foot.weight : 0;

    private Foot Solve(AvatarIKGoal goal, float bottomHeight)
    {
        var foot = new Foot { animated = animator.GetIKPosition(goal) };
        float soleHeight = foot.animated.y - bottomHeight - transform.position.y;
        foot.weight = 1 - Mathf.InverseLerp(plantedHeight, liftedHeight, soleHeight);
        Vector3 origin = foot.animated + Vector3.up * rayAbove;
        if (foot.weight > 0 && Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayAbove + rayBelow + bottomHeight, ground, QueryTriggerInteraction.Ignore))
        {
            foot.grounded = true;
            foot.groundPoint = hit.point;
            foot.groundNormal = Vector3.Slerp(Vector3.up, hit.normal, slopeFollow);
            foot.target = hit.point + foot.groundNormal * (bottomHeight - sink);
        }
        else foot.weight = 0;
        return foot;
    }

    private void Apply(AvatarIKGoal goal, Foot foot)
    {
        float w = foot.weight * weight;
        animator.SetIKPositionWeight(goal, w);
        animator.SetIKRotationWeight(goal, w);
        if (w <= 0) return;
        animator.SetIKPosition(goal, foot.target);
        //Flat on the ground, keeping the direction the animation gives the foot
        Vector3 forward = Vector3.ProjectOnPlane(animator.GetIKRotation(goal) * Vector3.forward, foot.groundNormal);
        if (forward.sqrMagnitude > 0.0001f) animator.SetIKRotation(goal, Quaternion.LookRotation(forward, foot.groundNormal));
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !enabled) return;
        DrawFoot(Left);
        DrawFoot(Right);
    }

    private void DrawFoot(Foot foot)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(foot.animated + Vector3.up * rayAbove, foot.animated + Vector3.down * rayBelow);
        if (!foot.grounded) return;
        Gizmos.color = Color.Lerp(Color.red, Color.green, foot.weight);
        Gizmos.DrawWireSphere(foot.target, 0.01f);
        Gizmos.DrawLine(foot.groundPoint, foot.groundPoint + foot.groundNormal * 0.05f);
    }
}
