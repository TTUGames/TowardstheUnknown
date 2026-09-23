using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Definition of an attack an enemy can use during its turn. Its name is the Wwise event posted when it is used.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyPattern", menuName = "TTU/Enemy Pattern")]
public class EnemyPatternData : ScriptableObject
{
    [BoxGroup("Targeting")] public EntityType targetType = EntityType.PLAYER;
    [BoxGroup("Targeting")] public TileSearchConfig range = new TileSearchConfig(TileSearchConfig.Shape.CircleAttack, 1, 1);

    [BoxGroup("Effects"), Tooltip("Applied to the target, in order")]
    [SerializeReference, ListDrawerSettings(ShowFoldout = false)] public List<CombatEffect> effects = new List<CombatEffect>();

    [BoxGroup("VFX"), Tooltip("Animator state played by the enemy, none if empty")] public string animStateName;
    [BoxGroup("VFX"), MinValue(0), SuffixLabel("s")] public float duration = 2f;
    [BoxGroup("VFX")] public List<VFXInfo> vfx = new List<VFXInfo>();
}

/// <summary>
/// A set of patterns, the first usable one being cast. The first one also defines where the enemy moves.
/// </summary>
[System.Serializable]
public class EnemyPatternSet
{
    [Tooltip("The distance the enemy tries to keep from its target")] public int targetDistance = 1;
    public List<EnemyPatternData> patterns = new List<EnemyPatternData>();
}
