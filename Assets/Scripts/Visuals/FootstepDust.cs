using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Puffs a little dust on the ground under the planted foot, the lowest one, at each step of the walk animation events.
/// The feet are the humanoid foot bones, or the bones named Foot or Hand (the paws) of a generic rig; they are found
/// again when the avatar or the model changes (Drareg's second phase). A change of deploy tile puffs at both tiles.
/// Each step also raises <see cref="Planted"/>, for the prints in the snow (<see cref="SnowPrints"/>)
/// </summary>
[RequireComponent(typeof(Animator))]
public class FootstepDust : MonoBehaviour
{
    [SerializeField, AssetsOnly, Tooltip("Emitted at the planted foot: world space, no emission of its own, local scaling: the entity's scale leaves it alone")] private ParticleSystem dustPrefab;
    [SerializeField, Min(1), Tooltip("Puffs per step")] private int count = 4;
    [SerializeField, Min(0.1f), Tooltip("Of the puffs, for the large entities")] private float scale = 1;
    [SerializeField, SuffixLabel("m"), Tooltip("Of the puffs above the entity's ground")] private float height = 0.04f;
    [SerializeField, Min(0), Tooltip("Puffs where the player leaves and where it lands when it changes deploy tile")] private int redeployCount = 12;
    [SerializeField, Min(0), SuffixLabel("m"), Tooltip("Length of the print a step leaves in the snow; 0 leaves none")] private float printSize = 0.16f;

    /// <summary>
    /// A foot planted on the ground: its point on the entity's ground, the way the entity faces (degrees around Y) and
    /// the length of its print
    /// </summary>
    public static event System.Action<Vector3, float, float> Planted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Planted = null;

    private Animator animator;
    private ParticleSystem dust;
    private Avatar feetAvatar;
    private readonly List<Transform> feet = new();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        dust = Instantiate(dustPrefab, transform);
        dust.transform.localScale = Vector3.one * scale;
    }

    private void OnEnable() => GameEvents.Redeployed += OnRedeployed;

    private void OnDisable() => GameEvents.Redeployed -= OnRedeployed;

    private void OnRedeployed(EntityStats entity, Vector3 from)
    {
        if (redeployCount == 0 || entity == null || entity != GetComponentInParent<EntityStats>()) return;
        Puff(from, redeployCount);
        Puff(entity.transform.position, redeployCount);
    }

    private void Puff(Vector3 position, int puffs)
    {
        position.y += height;
        dust.Emit(new ParticleSystem.EmitParams { position = position, applyShapeToPosition = true }, puffs);
    }

    // Called by the animation events, alongside FootstepAudio; they reach a disabled component too (the Classic edition)
    private void PlayFootstep()
    {
        if (!enabled) return;
        Transform foot = PlantedFoot();
        Vector3 position = foot != null ? foot.position : transform.position;
        position.y = transform.position.y;
        if (printSize > 0) Planted?.Invoke(position, transform.eulerAngles.y, printSize);
        position.y += height;
        dust.Emit(new ParticleSystem.EmitParams { position = position, applyShapeToPosition = true }, count);
    }

    private Transform PlantedFoot()
    {
        if (feetAvatar != animator.avatar || feet.Exists(foot => foot == null || !foot.gameObject.activeInHierarchy))
            FindFeet();
        Transform lowest = null;
        foreach (Transform foot in feet)
            if (lowest == null || foot.position.y < lowest.position.y)
                lowest = foot;
        return lowest;
    }

    private void FindFeet()
    {
        feetAvatar = animator.avatar;
        feet.Clear();
        if (animator.isHuman)
        {
            AddFoot(animator.GetBoneTransform(HumanBodyBones.LeftFoot));
            AddFoot(animator.GetBoneTransform(HumanBodyBones.RightFoot));
            return;
        }
        foreach (Transform bone in GetComponentsInChildren<Transform>())
            if (bone.name.Contains("Foot") || bone.name.Contains("Hand"))
                AddFoot(bone);
    }

    private void AddFoot(Transform foot)
    {
        if (foot != null)
            feet.Add(foot);
    }
}
