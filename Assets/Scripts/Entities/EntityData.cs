using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Identity of a kind of entity. Its name is its ID, the key of its localized name
/// </summary>
[CreateAssetMenu(fileName = "NewEntity", menuName = "TTU/Entity")]
public class EntityData : ScriptableObject
{
    [PreviewField(48), Tooltip("Icon in the turn timeline")] public Sprite timelineIcon;
    [Tooltip("Added to the score when it dies")] public int score;
    [AssetsOnly, Tooltip("Counted as this entity in the kills of the character sheet, as itself if none")] public EntityData killFamily;
    [Tooltip("Posted by the walk animation events")] public AK.Wwise.Event footstep = new AK.Wwise.Event();
    [AssetsOnly, Tooltip("Played now and then while it waits for the combat (IdleFidgets): looks around, sniffs, yawns")]
    public AnimationClip[] idleVariations = System.Array.Empty<AnimationClip>();
    [Tooltip("Lengthens the blends in and out of the idle variations, the attacks' times this: a slower settling")]
    public float idleBlendScale = 1;
    [Min(0), Tooltip("Speed of the walk clip so that the feet don't slide at the move's walking speed (TacticsMove), in an edition with EditionProfile.calibratedStrides; 0 keeps the prefab's (EntityAnimator.walkSpeed)")]
    public float strideWalkSpeed;
    [Min(0), Tooltip("Same for the run clip")]
    public float strideRunSpeed;
    [AssetsOnly, Tooltip("Other death clips, one drawn at random with the override controller's own at each death (EditionProfile.variedDeaths)")]
    public AnimationClip[] deathVariants = System.Array.Empty<AnimationClip>();
    [AssetsOnly, Tooltip("Hit reactions by the side the blow comes from, in the entity's frame: front, back, left, right; an empty one keeps the override controller's (EditionProfile.directionalHits)")]
    public AnimationClip[] directionalHits = new AnimationClip[4];
    [Tooltip("The original's height of the enemy info above the entity's feet (InfoEntity.downOffsetPercentage), in screen heights: read when EditionProfile.entityInfoOriginalAbove")]
    public float classicInfoOffset = 0.2f;

    public string ID => name;

    public EntityData KillFamily => killFamily != null ? killFamily : this;
}
