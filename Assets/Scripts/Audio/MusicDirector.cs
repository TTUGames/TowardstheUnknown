using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Switches the Wwise music states from the game events: exploration, combat and the boss phases. In a fight, the
/// CombatIntensity game parameter layers the combat music: its pulse percussion and bass ease as the enemies fall, and come
/// back whole while the player's health is low (EditionProfile.combatMusicLayers; otherwise the full mix, as the original's)
/// </summary>
public class MusicDirector : MonoBehaviour
{
    [SerializeField, Tooltip("Outside the combats")] private AK.Wwise.Event explore = new AK.Wwise.Event();
    [SerializeField, Tooltip("While a fight is on")] private AK.Wwise.Event combat = new AK.Wwise.Event();
    [SerializeField, Tooltip("Music of the rooms before the boss's")] private AK.Wwise.Event gameplay = new AK.Wwise.Event();
    [SerializeField, Tooltip("Music of the antechamber and of the boss")] private AK.Wwise.Event boss = new AK.Wwise.Event();
    [SerializeField, Tooltip("By boss phase: the first one for phase 1"), ListDrawerSettings(ShowIndexLabels = true)]
    private List<AK.Wwise.Event> bossPhases = new List<AK.Wwise.Event>();
    [SerializeField, Tooltip("CombatIntensity, 0 to 100: the combat music's pulse and bass, whole at 100")] private AK.Wwise.RTPC intensity = new AK.Wwise.RTPC();

    // The intensity with every enemy of the fight standing, and with the last one standing
    private const float FullFightIntensity = 80;
    private const float LastEnemyIntensity = 35;

    private int enemiesAtStart;
    private int enemiesStanding;
    private bool fighting;
    private PlayerStats player;

    private void OnEnable()
    {
        GameEvents.RoomEntered += OnRoomEntered;
        GameEvents.CombatEnded += OnCombatEnded;
        GameEvents.BossPhaseChanged += OnBossPhaseChanged;
        GameEvents.RunEnded += OnRunEnded;
        GameEvents.CombatStarted += OnCombatStarted;
        GameEvents.EntityDied += OnEntityDied;
        Edition.Changed += OnEditionChanged;
    }

    // The player's health, once its stats are known
    private void Start()
    {
        player = (PlayerStats)GameScene.Player.Stats;
        player.StatsChanged += RefreshIntensity;
    }

    private void OnDestroy()
    {
        if (player != null) player.StatsChanged -= RefreshIntensity;
    }

    private void OnDisable()
    {
        GameEvents.RoomEntered -= OnRoomEntered;
        GameEvents.CombatEnded -= OnCombatEnded;
        GameEvents.BossPhaseChanged -= OnBossPhaseChanged;
        GameEvents.RunEnded -= OnRunEnded;
        GameEvents.CombatStarted -= OnCombatStarted;
        GameEvents.EntityDied -= OnEntityDied;
        Edition.Changed -= OnEditionChanged;
        // A global game parameter: the next scene's music starts whole
        if (AkUnitySoundEngine.IsInitialized()) intensity.SetGlobalValue(100);
    }

    private void OnCombatStarted()
    {
        fighting = true;
        enemiesAtStart = enemiesStanding = Mathf.Max(1, TurnSystem.Instance.Turns.Count - 1);
        RefreshIntensity();
    }

    // Raised before the dead leaves the combat
    private void OnEntityDied(EntityStats entity)
    {
        if (entity is EnemyStats && fighting) enemiesStanding = Mathf.Max(0, enemiesStanding - 1);
        RefreshIntensity();
    }

    private void OnEditionChanged(GameEdition edition) => RefreshIntensity();

    /// <summary>
    /// From the share of the fight's enemies still standing, whole while the player's health is low or out of a fight
    /// </summary>
    private void RefreshIntensity()
    {
        if (!AkUnitySoundEngine.IsInitialized()) return;
        float value = 100;
        if (fighting && Edition.Profile.combatMusicLayers && (player == null || !player.IsHealthLow))
        {
            float share = enemiesAtStart > 1 ? (enemiesStanding - 1f) / (enemiesAtStart - 1) : 1;
            value = Mathf.Lerp(LastEnemyIntensity, FullFightIntensity, Mathf.Clamp01(share));
        }
        intensity.SetGlobalValue(value);
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

    private void OnCombatEnded()
    {
        fighting = false;
        RefreshIntensity();
        explore.Post(gameObject);
    }

    private void OnBossPhaseChanged(int phase)
    {
        if (phase >= 1 && phase <= bossPhases.Count) bossPhases[phase - 1].Post(gameObject);
    }

    private void OnRunEnded(bool isVictory)
    {
        if (isVictory) explore.Post(gameObject);
    }
}
