using UnityEngine;

/// <summary>
/// Switches the Wwise music states from the game events: exploration, combat and the boss phases
/// </summary>
public class MusicDirector : MonoBehaviour
{
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

    private void Post(string eventName) => AkUnitySoundEngine.PostEvent(eventName, gameObject);

    /// <summary>
    /// Switches the music depending on the room type and if a fight is going to start
    /// </summary>
    private void OnRoomEntered(Room room, bool firstVisit)
    {
        bool startsFight = firstVisit && room.GetComponentInChildren<EnemyStats>() != null;
        switch (room.type)
        {
            case RoomType.ANTECHAMBER:
                Post("SwitchExplore");
                Post("SwitchBoss");
                break;
            case RoomType.BOSS:
                if (!startsFight) break;
                Post("SwitchCombat");
                OnBossPhaseChanged(1);
                break;
            default:
                Post("SwitchGameplay");
                if (room.type == RoomType.COMBAT && startsFight) Post("SwitchCombat");
                break;
        }
    }

    private void OnCombatEnded() => Post("SwitchExplore");

    private void OnBossPhaseChanged(int phase) => Post("BossPhase" + phase);

    private void OnRunEnded(bool isVictory)
    {
        if (isVictory) Post("SwitchExplore");
    }
}
