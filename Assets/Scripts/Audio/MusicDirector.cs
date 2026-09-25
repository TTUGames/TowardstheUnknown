using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Switches the Wwise music states from the game events: exploration, combat and the boss phases
/// </summary>
public class MusicDirector : MonoBehaviour
{
    [SerializeField, Tooltip("Outside the combats")] private AK.Wwise.Event explore = new AK.Wwise.Event();
    [SerializeField, Tooltip("While a fight is on")] private AK.Wwise.Event combat = new AK.Wwise.Event();
    [SerializeField, Tooltip("Music of the rooms before the boss's")] private AK.Wwise.Event gameplay = new AK.Wwise.Event();
    [SerializeField, Tooltip("Music of the antechamber and of the boss")] private AK.Wwise.Event boss = new AK.Wwise.Event();
    [SerializeField, Tooltip("By boss phase: the first one for phase 1"), ListDrawerSettings(ShowIndexLabels = true)]
    private List<AK.Wwise.Event> bossPhases = new List<AK.Wwise.Event>();

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.CombatEnded += OnCombatEnded;
        GameEvents.BossPhaseChanged += OnBossPhaseChanged;
        GameEvents.RunEnded += OnRunEnded;
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.CombatEnded -= OnCombatEnded;
        GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
        GameEvents.RunEnded -= OnRunEnded;
    }

    /// <summary>
    /// Switches the music depending on the room type and if a fight is going to start
    /// </summary>
    private void OnRoomEntered(Room room, bool firstVisit)
    {
        bool startsFight = firstVisit && room.GetComponentInChildren<EnemyStats>() != null;
        switch (room.type)
        {
            case RoomType.ANTECHAMBER:
                explore.Post(gameObject);
                boss.Post(gameObject);
                break;
            case RoomType.BOSS:
                if (!startsFight) break;
                combat.Post(gameObject);
                OnBossPhaseChanged(1);
                break;
            default:
                gameplay.Post(gameObject);
                if (room.type == RoomType.COMBAT && startsFight) combat.Post(gameObject);
                break;
        }
    }

    private void OnCombatEnded() => explore.Post(gameObject);

    private void OnBossPhaseChanged(int phase)
    {
        if (phase >= 1 && phase <= bossPhases.Count) bossPhases[phase - 1].Post(gameObject);
    }

    private void OnRunEnded(bool isVictory)
    {
        if (isVictory) explore.Post(gameObject);
    }
}
