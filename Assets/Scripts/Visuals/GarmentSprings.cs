using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Swings the hanging parts of a character's clothes (the coat's flaps, the skirt): chains of bones the animation doesn't
/// move, each bone springing back towards its rest direction under the body, with inertia, a drag and a gravity, and pushed
/// out of the capsules of the legs. The rest of each chain follows the swing of the thigh on its side (<see cref="Chain.follow"/>:
/// the front the most): a stepping leg carries the flaps in front of it instead of going through them. An inner layer (the
/// skirt under the coat) copies the swing of the outer one (<see cref="Chain.leader"/>), so that the two never cross. Stylized rather than real: the flaps flare out a little as if lifted by magic and
/// undulate slowly, each chain in its own phase, and the spring overshoots; the room's wind (<see cref="Wind.At"/>) blows them,
/// much more as a gust passes. Runs after the animation and the foot IK, in
/// scaled time (a hit stop freezes the cloth, a slow motion slows it). The rest is the chain's pose in the prefab, restored
/// while it is turned off (the Classic edition's <see cref="EditionOnly"/>, whose clothes are the original's meshes, not
/// skinned to these bones)
/// </summary>
[DefaultExecutionOrder(1000)]
public class GarmentSprings : MonoBehaviour
{
    [System.Serializable]
    public class Capsule
    {
        [Tooltip("Ends of the capsule's segment: a thigh's hip and knee")] public Transform start, end;
        [SuffixLabel("m")] public float radius = 0.06f;
    }

    [System.Serializable]
    public class Chain
    {
        [Tooltip("The first bone, under the pelvis: its descendants, one child each, make the chain and the last one is its tip")] public Transform root;
        [Tooltip("The thigh on the chain's side, whose swing it follows")] public Transform thigh;
        [Range(0, 1), Tooltip("How much of the thigh's swing the chain's rest follows")] public float follow;
        [Tooltip("The thigh's rotation in the pelvis's space at the bind pose, where the chain's rest was made")] public Quaternion thighRest = Quaternion.identity;
        [Tooltip("The first bone of the outer chain this inner one copies, bone for bone: it swings with it, pushed out of the legs only")] public Transform leader;
    }

    [SerializeField] private Chain[] chains = System.Array.Empty<Chain>();
    [SerializeField] private Capsule[] colliders = System.Array.Empty<Capsule>();

    [BoxGroup("Spring"), SerializeField, Min(0), Tooltip("Pull back towards the rest direction, per second")] private float stiffness = 2.2f;
    [BoxGroup("Spring"), SerializeField, Range(0, 1), Tooltip("Share of the velocity lost each frame: low, the flaps overshoot and bounce")] private float drag = 0.18f;
    [BoxGroup("Spring"), SerializeField, SuffixLabel("m/s²"), Tooltip("Pulls the tips down, in meters")] private float gravity = 0.4f;
    [BoxGroup("Spring"), SerializeField, Range(0, 180), Tooltip("The most a bone turns from its rest direction")] private float maxAngle = 45;
    [BoxGroup("Spring"), SerializeField, SuffixLabel("m"), Tooltip("The bones' thickness against the capsules")] private float boneRadius = 0.02f;
    [BoxGroup("Spring"), SerializeField, SuffixLabel("m"), Tooltip("A move of the character longer than this in one frame (a room change) starts the chains again from rest")]
    private float teleportDistance = 0.5f;

    [BoxGroup("Magic"), SerializeField, Range(0, 45), Tooltip("The rest direction turned outwards, in degrees: the flaps flare as if lifted")] private float flare = 6;
    [BoxGroup("Magic"), SerializeField, Range(0, 30), Tooltip("The slow undulation of the flaps around their rest, in degrees")] private float undulation = 5;
    [BoxGroup("Magic"), SerializeField, SuffixLabel("Hz")] private float undulationSpeed = 0.45f;
    [BoxGroup("Magic"), SerializeField, Range(0, 3), Tooltip("Phase added down a chain, in radians: the undulation runs to the hem like a wave")] private float undulationLag = 0.9f;

    [BoxGroup("Wind"), SerializeField, Min(0), Tooltip("How much the room's wind pushes the flaps, against the stiffness: the breeze stirs them, a gust lifts them")]
    private float windResponse = 0.8f;

    private class Joint
    {
        public Transform bone;
        public Chain chain;
        public int depth;
        public float phase;
        public Quaternion restLocalRotation;
        // The tail in the bone's space: the next bone of the chain
        public Vector3 axis;
        public float length;
        public Vector3 tail, previousTail;
        // The joint of the outer chain at the same depth (or its last), for an inner chain
        public Joint leader;
    }

    private readonly List<Joint> joints = new();
    private Vector3 lastPosition;

    private void Awake()
    {
        for (int c = 0; c < chains.Length; c++)
        {
            Chain chain = chains[c];
            if (chain.root == null) continue;
            int depth = 0;
            for (Transform bone = chain.root; bone.childCount > 0; bone = bone.GetChild(0), depth++)
            {
                Transform next = bone.GetChild(0);
                joints.Add(new Joint {
                    bone = bone, chain = chain, depth = depth, phase = c * 2.399f,
                    restLocalRotation = bone.localRotation, axis = next.localPosition,
                });
            }
        }
        foreach (Joint joint in joints)
        {
            if (joint.chain.leader == null) continue;
            Joint leader = null;
            foreach (Joint candidate in joints)
                if (candidate.chain.root == joint.chain.leader && candidate.depth <= joint.depth && (leader == null || candidate.depth > leader.depth))
                    leader = candidate;
            joint.leader = leader;
        }
        // The outer chains first: the inner ones copy them
        joints.Sort((a, b) => (a.leader != null).CompareTo(b.leader != null));
    }

    private void OnEnable() => ResetChains();

    // Back to rest: the bones keep the rotation last written otherwise
    private void OnDisable()
    {
        foreach (Joint joint in joints) joint.bone.localRotation = joint.restLocalRotation;
    }

    /// <summary>
    /// Puts the chains back at rest, still
    /// </summary>
    public void ResetChains()
    {
        lastPosition = transform.position;
        foreach (Joint joint in joints) joint.bone.localRotation = joint.restLocalRotation;
        foreach (Joint joint in joints)
        {
            joint.tail = joint.previousTail = joint.bone.TransformPoint(joint.axis);
            joint.length = Vector3.Distance(joint.bone.position, joint.tail);
        }
    }

    private void LateUpdate()
    {
        if ((transform.position - lastPosition).sqrMagnitude > teleportDistance * teleportDistance)
        {
            ResetChains();
            return;
        }
        lastPosition = transform.position;
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0) return;
        float time = Time.time * undulationSpeed * 2 * Mathf.PI;
        Vector3 wind = Wind.At(transform.position) * windResponse;

        // Parents first: a bone's rest follows its parent's new rotation
        foreach (Joint joint in joints)
        {
            Transform bone = joint.bone;
            if (joint.leader != null)
            {
                FollowLeader(joint);
                continue;
            }
            bone.localRotation = joint.depth == 0 ? ThighSwing(joint.chain) * joint.restLocalRotation : joint.restLocalRotation;
            Vector3 head = bone.position;
            Vector3 restTail = bone.TransformPoint(joint.axis);
            Vector3 restDirection = (restTail - head).normalized;
            // Away from the pelvis, the chain's parent
            Vector3 outward = Vector3.ProjectOnPlane(joint.chain.root.position - joint.chain.root.parent.position, Vector3.up);
            if (outward.sqrMagnitude > 1e-8f)
            {
                float angle = flare + undulation * Mathf.Sin(time + joint.phase - joint.depth * undulationLag);
                Vector3 axis = Vector3.Cross(restDirection, outward);
                if (axis.sqrMagnitude > 1e-8f) restDirection = Quaternion.AngleAxis(angle, axis.normalized) * restDirection;
            }

            Vector3 next = joint.tail
                + (joint.tail - joint.previousTail) * (1 - drag)
                + (restDirection * stiffness + wind) * (deltaTime * joint.length)
                + Vector3.down * (gravity * deltaTime * deltaTime);
            next = head + (next - head).normalized * joint.length;
            // The tip and the middle of the bone out of the legs
            foreach (Capsule capsule in colliders)
            {
                next = PushOut(head, next, joint.length, capsule);
                Vector3 middle = (head + next) / 2;
                Vector3 pushed = PushOut(head, middle, joint.length / 2, capsule);
                if (pushed != middle) next = head + (pushed - head).normalized * joint.length;
            }

            // Not further than maxAngle from the rest
            Vector3 direction = next - head;
            float fromRest = Vector3.Angle(restDirection, direction);
            if (fromRest > maxAngle)
                direction = Vector3.Slerp(restDirection, direction.normalized, maxAngle / fromRest);
            next = head + direction.normalized * joint.length;

            joint.previousTail = joint.tail;
            joint.tail = next;
            bone.rotation = Quaternion.FromToRotation(restTail - head, next - head) * bone.rotation;
        }
    }

    // An inner layer's bone turns as the outer one's does from its rest, then leaves the legs
    private void FollowLeader(Joint joint)
    {
        Transform bone = joint.bone;
        Quaternion swing = joint.leader.bone.localRotation * Quaternion.Inverse(joint.leader.restLocalRotation);
        bone.localRotation = swing * joint.restLocalRotation;
        Vector3 head = bone.position;
        Vector3 tail = bone.TransformPoint(joint.axis);
        Vector3 pushed = tail;
        foreach (Capsule capsule in colliders)
            pushed = PushOut(head, pushed, joint.length, capsule);
        if (pushed != tail) bone.rotation = Quaternion.FromToRotation(tail - head, pushed - head) * bone.rotation;
        joint.previousTail = joint.tail = bone.TransformPoint(joint.axis);
    }

    // The thigh's turn since the bind pose, in the pelvis's space, the chain's share of it
    private static Quaternion ThighSwing(Chain chain)
    {
        if (chain.thigh == null || chain.follow <= 0) return Quaternion.identity;
        Quaternion now = Quaternion.Inverse(chain.root.parent.rotation) * chain.thigh.rotation;
        return Quaternion.Slerp(Quaternion.identity, now * Quaternion.Inverse(chain.thighRest), chain.follow);
    }

    private Vector3 PushOut(Vector3 head, Vector3 tail, float length, Capsule capsule)
    {
        if (capsule.start == null || capsule.end == null) return tail;
        Vector3 a = capsule.start.position, b = capsule.end.position;
        Vector3 ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(tail - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f));
        Vector3 closest = a + ab * t;
        Vector3 away = tail - closest;
        float reach = capsule.radius + boneRadius;
        if (away.sqrMagnitude >= reach * reach) return tail;
        Vector3 pushed = closest + (away.sqrMagnitude > 1e-8f ? away.normalized : (tail - head).normalized) * reach;
        return head + (pushed - head).normalized * length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (Chain chain in chains)
            for (Transform bone = chain.root; bone != null && bone.childCount > 0; bone = bone.GetChild(0))
                Gizmos.DrawLine(bone.position, bone.GetChild(0).position);
        Gizmos.color = Color.yellow;
        foreach (Capsule capsule in colliders)
        {
            if (capsule.start == null || capsule.end == null) continue;
            Gizmos.DrawWireSphere(capsule.start.position, capsule.radius);
            Gizmos.DrawWireSphere(capsule.end.position, capsule.radius);
            Gizmos.DrawLine(capsule.start.position, capsule.end.position);
        }
    }
}
