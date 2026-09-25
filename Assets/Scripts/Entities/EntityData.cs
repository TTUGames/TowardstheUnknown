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

    public string ID => name;

    public EntityData KillFamily => killFamily != null ? killFamily : this;
}
