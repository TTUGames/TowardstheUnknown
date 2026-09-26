using UnityEngine;

/// <summary>
/// The attack mode of the player: shows the range of the selected artifact and its targets under the pointer, and casts it on the clicked tile.
/// Casts clicked during another one are paid and queued, then launched one after the other.
/// </summary>
public class PlayerAttack : MonoBehaviour, IPlayerMode
{
    public InventoryManager inventory;
    private PlayerStats playerStats;
    private PlayerTurn playerTurn;

    public Artifact currentArtifact { get; private set; }

    /// <summary>
    /// Fired when the player selects an artifact it can't cast: not enough energy, cooldown or no use left this turn
    /// </summary>
    public event System.Action<Artifact> ArtifactRefused;

    /// <summary>
    /// Fired when the entities the selected artifact would hit change, with none when the targeting stops
    /// </summary>
    public event System.Action<Artifact, System.Collections.Generic.IReadOnlyList<EntityStats>> TargetsPreviewed;

    private readonly System.Collections.Generic.List<EntityStats> previewedTargets = new();

    /// <summary>
    /// A cast clicked while another one plays: on the clicked entity, which it follows if it moves, else on the clicked tile
    /// </summary>
    public readonly struct QueuedCast
    {
        public readonly Artifact Artifact;
        public readonly Tile Tile;
        public readonly TacticsMove Target;

        public QueuedCast(Artifact artifact, Tile tile, TacticsMove target)
        {
            Artifact = artifact;
            Tile = tile;
            Target = target;
        }

        public Tile TargetedTile => Target != null ? Target.CurrentTile : Tile;
    }

    private readonly System.Collections.Generic.List<QueuedCast> queuedCasts = new();

    /// <summary>
    /// The casts waiting for the current one to end, in order
    /// </summary>
    public System.Collections.Generic.IReadOnlyList<QueuedCast> QueuedCasts => queuedCasts;

    /// <summary>
    /// Fired when a cast is queued, launched or dropped
    /// </summary>
    public event System.Action QueueChanged;

    /// <summary>
    /// Whether a cast or a chain of casts plays, during which the player can aim and queue the next ones
    /// </summary>
    public bool IsCasting { get; private set; }

    [SerializeField, Tooltip("Seconds of an attack's recovery kept when a queued cast follows it")] private float chainedRecovery = 0.1f;

    //Whether the selected artifact's range is shown and a click casts it
    private bool aiming;
    private bool endTurnRequested;

    [SerializeField] private Transform leftHandMarker;
    [SerializeField] private Transform rightHandMarker;
    [SerializeField] private Transform gunMarker;
    [SerializeField] private Transform swordMarker;
    [SerializeField] private Transform backMarker;

    private Dissolving dissolving;
    private PlayerGlow glow;
    private TacticsMove tacticsMove;

    private void Start()
    {
        tacticsMove = GetComponent<TacticsMove>();
        inventory = GetComponent<InventoryManager>();
        playerStats = GetComponent<PlayerStats>();
        playerTurn = GetComponent<PlayerTurn>();
        dissolving = GetComponent<Dissolving>();
        glow = GetComponent<PlayerGlow>();
    }

    /// <summary>
    /// Shows the tiles the artifact would hit
    /// </summary>
    public void OnTileHovered(Tile hoveredTile)
    {
        Tile.ResetTargetTiles();
        previewedTargets.Clear();
        if (hoveredTile != null && hoveredTile.Selection == Tile.SelectionType.ATTACK)
            foreach (Tile tile in currentArtifact.GetTargets(hoveredTile))
            {
                tile.IsTarget = true;
                TacticsMove entity = tile.GetEntity();
                if (entity != null && entity.TryGetComponent(out EntityStats stats) && stats.type == currentArtifact.Target) previewedTargets.Add(stats);
            }
        TargetsPreviewed?.Invoke(currentArtifact, previewedTargets);
    }

    private void StopPreview()
    {
        previewedTargets.Clear();
        TargetsPreviewed?.Invoke(null, previewedTargets);
    }

    public void OnTileClicked(Tile tile) => Attack(tile);

    /// <summary>
    /// Pays the selected <c>Artifact</c> and casts it on the tile, or queues it if another cast plays.
    /// Goes back to moving once the last queued cast is done.
    /// </summary>
    /// <param name="tile">The tile the player clicked</param>
    public void Attack(Tile tile)
    {
        if (!aiming) return;
        if (!currentArtifact.CanTarget(tile))
        {
            if (tile != null) playerTurn.RefuseClick(tile);
            return;
        }
        aiming = false;
        currentArtifact.Pay(playerStats); //Spending energy refreshes the energy and skills UI
        playerStats.PreviewEnergyCost(0);
        Tile.ResetTiles();
        StopPreview();
        if (IsCasting)
        {
            //An area keeps its tile, a single target is followed if it moves
            TacticsMove target = currentArtifact.IsAreaOfEffect ? null : tile.GetEntity();
            queuedCasts.Add(new QueuedCast(currentArtifact, tile, target));
            QueueChanged?.Invoke();
            return;
        }
        IsCasting = true;
        Cast(currentArtifact, tile);
    }

    private void Cast(Artifact artifact, Tile tile)
    {
        glow.Colorize(artifact.Color);
        dissolving.Undissolve(artifact.Weapon);
        artifact.Cast(playerStats, tile, HasQueuedCasts, chainedRecovery);
        ActionManager.WhenFree(OnCastEnd);
    }

    private bool HasQueuedCasts() => queuedCasts.Count > 0;

    /// <summary>
    /// Launches the next queued cast still valid, refunding the others, else ends the chain
    /// </summary>
    private void OnCastEnd()
    {
        //The player can die from its own attack
        if (playerStats.CurrentHealth <= 0) return;
        TurnSystem turns = TurnSystem.Instance;
        //The last enemy may have died: the combat ends, the rest of the chain has nothing to do
        bool playing = turns.IsCombat && turns.IsPlayerTurn && turns.Turns.Count > 1;
        while (queuedCasts.Count > 0)
        {
            QueuedCast next = queuedCasts[0];
            queuedCasts.RemoveAt(0);
            QueueChanged?.Invoke();
            if (playing && CanStillCast(next))
            {
                Cast(next.Artifact, next.TargetedTile);
                return;
            }
            next.Artifact.Refund(playerStats);
            if (playing) ArtifactRefused?.Invoke(next.Artifact);
        }
        IsCasting = false;
        dissolving.Start();
        if (endTurnRequested)
        {
            endTurnRequested = false;
            if (playing)
            {
                turns.EndPlayerTurn();
                return;
            }
        }
        //The artifact aimed during the chain keeps its aim, its range from where the player now stands
        if (aiming) CheckAndPreviewArtifact();
        else
        {
            playerStats.PreviewEnergyCost(0);
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
        }
    }

    /// <summary>
    /// A queued cast still has a target in range: the clicked entity alive, or the clicked tile
    /// </summary>
    private bool CanStillCast(QueuedCast cast)
    {
        //A destroyed entity compares equal to null, but not by reference
        if (!ReferenceEquals(cast.Target, null) && (cast.Target == null || cast.Target.GetComponent<EntityStats>().IsDead)) return false;
        Tile tile = cast.TargetedTile;
        return tile != null && cast.Artifact.CanTarget(tile) && cast.Artifact.CanReach(CurrentTile, tile);
    }

    /// <summary>
    /// Drops the queued casts and gives back their costs
    /// </summary>
    public void CancelQueuedCasts()
    {
        endTurnRequested = false;
        if (queuedCasts.Count == 0) return;
        for (int i = queuedCasts.Count - 1; i >= 0; i--)
            queuedCasts[i].Artifact.Refund(playerStats);
        queuedCasts.Clear();
        QueueChanged?.Invoke();
    }

    /// <summary>
    /// Ends the turn once the casts playing and queued are done
    /// </summary>
    /// <returns>Whether the end of the turn waits for them</returns>
    public bool DeferEndTurn()
    {
        if (!IsCasting) return false;
        endTurnRequested = true;
        return true;
    }

    /// <summary>
    /// Selects the artifact to attack with
    /// </summary>
    /// <param name="numArtifact">the index of the <c>Artifact</c> to attack with</param>
    public void SetAttackingArtifact(int numArtifact)
    {
        var artifacts = inventory.GetPlayerArtifacts();
        if (numArtifact >= artifacts.Count)
        {
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
            return;
        }
        currentArtifact = artifacts[numArtifact];
        CheckAndPreviewArtifact();
    }

    /// <summary>
    /// Checks if the currentArtifact can still be cast, and previews its range and energy cost if it can. Else, does to move state.
    /// </summary>
    private void CheckAndPreviewArtifact()
    {
        if (!currentArtifact.CanUse(playerStats))
        {
            aiming = false;
            ArtifactRefused?.Invoke(currentArtifact);
            playerStats.PreviewEnergyCost(0);
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
            return;
        }
        Tile.ResetTiles();
        aiming = true;

        TileSearch range = currentArtifact.Range;
        range.SetStartingTile(CurrentTile);
        range.Search();
        foreach (Tile tile in range.GetTiles()) tile.Selection = Tile.SelectionType.ATTACK;
        //Also when the hovered tile is out of this artifact's range: the previous artifact's targets must go
        OnTileHovered(Room.HoveredTile);
        playerStats.PreviewEnergyCost(currentArtifact.Cost);
    }

    // The range is shown once an artifact is selected
    public void Enter() { }

    public void Exit()
    {
        aiming = false;
        Tile.ResetTiles();
        StopPreview();
    }

    /// <summary>
    /// Ends the visuals of an attack, called when any attack animation ends
    /// </summary>
    public void EndAttackVisuals()
    {
        glow.Uncolorize();
        dissolving.DissolveAll();
    }

    /// <summary>
    /// The marker of the body a VFX starts from
    /// </summary>
    public Transform Anchor(VFXInfo.Target target) => target switch
    {
        VFXInfo.Target.GUN => gunMarker,
        VFXInfo.Target.SWORD => swordMarker,
        VFXInfo.Target.LEFTHAND => leftHandMarker,
        VFXInfo.Target.RIGHTHAND => rightHandMarker,
        _ => backMarker,
    };

    public PlayerStats Stats => playerStats;

    public Tile CurrentTile => tacticsMove.CurrentTile;
}
