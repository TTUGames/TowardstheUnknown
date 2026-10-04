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
    [Tooltip("The original's height of the enemy info above the entity's feet (InfoEntity.downOffsetPercentage), in screen heights: read when EditionProfile.entityInfoOriginalAbove")]
    public float classicInfoOffset = 0.2f;

    public string ID => name;

    public EntityData KillFamily => killFamily != null ? killFamily : this;
}
