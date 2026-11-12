using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Turns the entity's head (and a little of its chest) towards an opponent standing in front of it, during the deploy
/// phase and the combat: the opponent playing its turn, on the player's turn the enemy it hovers or aims at, otherwise
/// the nearest one, preferably in front. The head turns at most <c>maxAngle</c> from the way the entity faces: an opponent
/// further to the side is looked towards as far as the head goes. One behind, past <c>awareAngle</c>, is not watched; the
/// look fades out towards that edge, during an attack (its clip aims the body) and on death.
/// A humanoid looks through the animator's look-at (the IK pass of its layers); a generic rig (the wolves, the bears)
/// turns its <c>bones</c> after the animation, each by its share of the angle.
/// </summary>
[RequireComponent(typeof(Animator))]
public class LookAtIK : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float weight = 1;
    [SerializeField, Range(0, 90), SuffixLabel("°"), Tooltip("The most the head turns from the way the entity faces: an opponent further to the side is looked towards as far as this")]
    private float maxAngle = 70;
    [SerializeField, Range(0, 180), SuffixLabel("°"), Tooltip("An opponent further than this from the way the entity faces, behind it, is not watched")]
    private float awareAngle = 150;
    [SerializeField, Range(0, 45), SuffixLabel("°"), Tooltip("Over the last degrees before awareAngle, the look fades out")]
    private float edgeFade = 20;
    [SerializeField, Range(0, 60), SuffixLabel("°"), Tooltip("The most the head looks up or down")]
    private float maxPitch = 30;
    [SerializeField, SuffixLabel("m"), Tooltip("An opponent further away is not watched")]
    private float maxDistance = 12;

    [BoxGroup("Timing"), SerializeField, SuffixLabel("s"), Tooltip("Time to turn the head towards a new target")] private float followTime = 0.3f;
    [BoxGroup("Timing"), SerializeField, SuffixLabel("s"), Tooltip("Time to blend the look in")] private float blendIn = 0.4f;
    [BoxGroup("Timing"), SerializeField, SuffixLabel("s"), Tooltip("Time to blend the look out")] private float blendOut = 0.25f;

    [BoxGroup("Humanoid"), SerializeField, Range(0, 1), Tooltip("How much the chest turns with the head")] private float bodyWeight = 0.25f;
    [BoxGroup("Humanoid"), SerializeField, Range(0, 1)] private float headWeight = 0.9f;
    [BoxGroup("Humanoid"), SerializeField, Range(0, 1)] private float eyesWeight = 1f;

    [BoxGroup("Generic rig"), SerializeField, Tooltip("The bones turned on a generic rig, from the root to the head, and the share of the angle each takes (the shares adding up to 1). Empty: the bones named Neck and Head, half each")]
    private List<Bone> bones = new();
    [BoxGroup("Generic rig"), SerializeField, SuffixLabel("m"), Tooltip("Height of the eyes above the feet, on a rig without a head bone")]
    private float eyesHeight = 1;

    [System.Serializable]
    private struct Bone
    {
        public Transform transform;
        [Range(0, 1)] public float share;
    }

    private Animator animator;
    private EntityTurn turn;
    private EntityStats stats;
    private EntityAnimator entityAnimator;
    private PlayerAttack playerAttack;
    private EntityStats aimed;
    private Transform head;
    private Avatar headAvatar;
    private Vector3 lookPoint;
    private Vector3 lookVelocity;
    private float current;

    /// <summary>
    /// The point between the eyes, which the other entities look at
    /// </summary>
    public Vector3 Eyes => Head() != null ? head.position : Body.position + Vector3.up * eyesHeight;

    // The entity's root, which the tiles move and turn: the model's animator may sit on a child
    private Transform Body => stats != null ? stats.transform : transform;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        turn = GetComponentInParent<EntityTurn>();
        stats = GetComponentInParent<EntityStats>();
        entityAnimator = GetComponent<EntityAnimator>();
    }

    private void OnEnable()
    {
        if (turn is PlayerTurn && turn.TryGetComponent(out playerAttack))
            playerAttack.TargetsPreviewed += OnTargetsPreviewed;
    }

    // Turned off (the Classic edition), the animation keeps the head: start again from nothing
    private void OnDisable()
    {
        if (playerAttack != null) playerAttack.TargetsPreviewed -= OnTargetsPreviewed;
        playerAttack = null;
        aimed = null;
        current = 0;
        lookVelocity = Vector3.zero;
    }

    private void OnTargetsPreviewed(Artifact artifact, IReadOnlyList<EntityStats> targets) =>
        aimed = targets != null && targets.Count > 0 ? targets[0] : null;

    private void Update()
    {
        EntityStats target = PickTarget();
        bool busy = stats == null || stats.IsDead || (entityAnimator != null && entityAnimator.IsAttacking);
        float wanted = 0;
        if (target != null && !busy)
        {
            Vector3 eyes = EyesOf(target);
            Vector3 point = Aim(eyes);
            //A new target after a pause is looked at directly; another one while looking, the head turns to it
            if (current <= 0) lookPoint = point;
            else lookPoint = Vector3.SmoothDamp(lookPoint, point, ref lookVelocity, followTime);
            wanted = weight * Mathf.InverseLerp(awareAngle, awareAngle - edgeFade, Yaw(eyes));
        }
        float duration = wanted > current ? blendIn : blendOut;
        current = Mathf.MoveTowards(current, wanted, Time.deltaTime / Mathf.Max(duration, 0.001f));
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!enabled || !animator.isHuman) return;
        animator.SetLookAtWeight(current, bodyWeight, headWeight, eyesWeight, 0.5f);
        if (current > 0) animator.SetLookAtPosition(lookPoint);
    }

    // A generic rig turns its bones after the animation, which sets them again at each frame
    private void LateUpdate()
    {
        if (current <= 0 || animator.isHuman) return;
        if (bones.Count == 0) FindBones();
        Vector3 from = Eyes;
        Vector3 to = lookPoint - from;
        Vector3 flat = Vector3.ProjectOnPlane(to, Vector3.up);
        if (flat.sqrMagnitude < 1e-6f) return;
        float yaw = Vector3.SignedAngle(Vector3.ProjectOnPlane(Body.forward, Vector3.up), flat, Vector3.up);
        Vector3 side = Vector3.Cross(flat, Vector3.up).normalized;
        float pitch = Vector3.SignedAngle(flat, to, side);
        foreach (Bone bone in bones)
        {
            if (bone.transform == null) continue;
            float share = bone.share * current;
            //Around the world's up for the yaw and the side of the look for the pitch: whatever axes the bones were built along
            Quaternion turnBy = Quaternion.AngleAxis(yaw * share, Vector3.up) * Quaternion.AngleAxis(pitch * share, side);
            bone.transform.rotation = turnBy * bone.transform.rotation;
        }
    }

    private EntityStats PickTarget()
    {
        TurnSystem turns = TurnSystem.Instance;
        if (turn == null || turns == null || !(turns.IsCombat || PlayerDeploy.InDeployPhase)) return null;

        //The opponent playing its turn
        EntityTurn playing = turns.Current;
        if (playing != null && IsOpponent(playing) && Watchable(playing.stats)) return playing.stats;

        //On the player's turn, the enemy it aims at or hovers
        if (playing == turn)
        {
            if (Watchable(aimed)) return aimed;
            TacticsMove hovered = BoardPointer.HoveredEntity;
            if (hovered != null && Watchable(hovered.Stats) && IsOpponent(hovered.GetComponent<EntityTurn>())) return hovered.Stats;
        }

        //Otherwise the nearest one in front
        EntityStats nearest = null;
        float nearestDistance = float.MaxValue;
        foreach (EntityTurn other in turns.Turns)
        {
            if (!IsOpponent(other) || !Watchable(other.stats)) continue;
            //One in front, where the head turns all the way, before one to the side or behind
            float distance = (other.transform.position - Body.position).sqrMagnitude;
            if (Yaw(EyesOf(other.stats)) > maxAngle) distance += 1e4f;
            if (distance < nearestDistance)
            {
                nearest = other.stats;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    private bool IsOpponent(EntityTurn other) => other != null && other != turn && (other is PlayerTurn) != (turn is PlayerTurn);

    private bool Watchable(EntityStats other)
    {
        if (other == null || other == stats || other.IsDead || !other.isActiveAndEnabled) return false;
        Vector3 point = EyesOf(other);
        return (point - Body.position).sqrMagnitude <= maxDistance * maxDistance && Yaw(point) < awareAngle;
    }

    // The angle, around the vertical, between the way the entity faces and a point
    private float Yaw(Vector3 point)
    {
        Vector3 flat = Vector3.ProjectOnPlane(point - Body.position, Vector3.up);
        return flat.sqrMagnitude < 1e-6f ? 0 : Vector3.Angle(Vector3.ProjectOnPlane(Body.forward, Vector3.up), flat);
    }

    // The point looked at towards another one, as far as the head goes: maxAngle to the side, maxPitch up or down
    private Vector3 Aim(Vector3 point)
    {
        Vector3 from = Eyes;
        Vector3 to = point - from;
        Vector3 flat = Vector3.ProjectOnPlane(to, Vector3.up);
        Vector3 facing = Vector3.ProjectOnPlane(Body.forward, Vector3.up);
        if (flat.sqrMagnitude < 1e-6f || facing.sqrMagnitude < 1e-6f) return point;
        float yaw = Mathf.Clamp(Vector3.SignedAngle(facing, flat, Vector3.up), -maxAngle, maxAngle);
        float pitch = Mathf.Clamp(Mathf.Atan2(to.y, flat.magnitude) * Mathf.Rad2Deg, -maxPitch, maxPitch);
        Vector3 side = Quaternion.AngleAxis(yaw, Vector3.up) * facing.normalized;
        return from + to.magnitude * (side * Mathf.Cos(pitch * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(pitch * Mathf.Deg2Rad));
    }

    private static Vector3 EyesOf(EntityStats other)
    {
        LookAtIK look = other.GetComponentInChildren<LookAtIK>();
        return look != null ? look.Eyes : other.transform.position + Vector3.up;
    }

    // The head bone, found again when the avatar changes (Drareg's second phase)
    private Transform Head()
    {
        if (head != null && headAvatar == animator.avatar && head.gameObject.activeInHierarchy) return head;
        headAvatar = animator.avatar;
        head = animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : FindBone("Head");
        return head;
    }

    private void FindBones()
    {
        Transform neck = FindBone("Neck");
        Transform headBone = FindBone("Head");
        if (neck != null) bones.Add(new Bone { transform = neck, share = headBone != null ? 0.5f : 1 });
        if (headBone != null) bones.Add(new Bone { transform = headBone, share = neck != null ? 0.5f : 1 });
    }

    private Transform FindBone(string boneName)
    {
        foreach (Transform bone in GetComponentsInChildren<Transform>())
            if (bone.name == boneName && bone.gameObject.activeInHierarchy) return bone;
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !enabled || current <= 0) return;
        Gizmos.color = Color.Lerp(Color.red, Color.cyan, current / Mathf.Max(weight, 0.001f));
        Gizmos.DrawLine(Eyes, lookPoint);
        Gizmos.DrawWireSphere(lookPoint, 0.05f);
    }
}
