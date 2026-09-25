using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Definition of an attack an enemy can use during its turn
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyPattern", menuName = "TTU/Enemy Pattern")]
public class EnemyPatternData : AbilityData
{
    private void Reset() => target = EntityType.PLAYER;
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
