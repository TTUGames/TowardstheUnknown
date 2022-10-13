using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Artifact : IArtifact
{
    [SerializeField] private GameObject prefab;

    [SerializeField] protected int cost;

    [SerializeField] protected int distanceMin;
    [SerializeField] protected int distanceMax;

    [SerializeField] protected int   maximumUsePerTurn;
    [SerializeField] protected int   cooldown;
    [SerializeField] protected float lootRate;

    protected int remainingUsesThisTurn;
    protected int remainingCooldown;
    protected bool wasUsedSinceLastTurnStart;

    [SerializeField] protected Vector2 size;
    protected List<string> targets = new List<string>();


    protected List<Pair<IAction, ActionTarget>> actions = new List<Pair<IAction, ActionTarget>>();

    /// <summary>
    /// Sets all the basic values for the artifact.
    /// </summary>
    /// <param name="cost">The artifact's energy cost</param>
    /// <param name="distanceMin">The minimum cast distance</param>
    /// <param name="distanceMax">The Maximum cast distance</param>
    /// <param name="maximumUsePerTurn">The maximum number of uses per turn</param>
    /// <param name="cooldown">The number of turns the player must wait before using this artifact again</param>
    /// <param name="size">The artifact's size in the inventory</param>
    /// <param name="lootRate">The artifact's lootrate</param>
    protected void SetValues(int cost, int distanceMin, int distanceMax, int maximumUsePerTurn, int cooldown, Vector2 size, float lootRate) {
        this.cost = cost;
        this.distanceMin = distanceMin;
        this.distanceMax = distanceMax;
        this.maximumUsePerTurn = maximumUsePerTurn;
        this.cooldown = cooldown;
        this.size = size;
        this.lootRate = lootRate;
	}

    /// <summary>
    /// Registers an action for the artifact, that will be used when the artifact is used, in the order the actions were registered
    /// </summary>
    /// <param name="action">The action to register</param>
    /// <param name="target">The entity the action must target (source or target of the artifact)</param>
    protected void AddAction(IAction action, ActionTarget target) {
        actions.Add(new Pair<IAction, ActionTarget>(action, target));
	}

    /// <summary>
    /// Applies every action of the artifact
    /// </summary>
    /// <param name="source">The entity using the artifact</param>
    /// <param name="target">The entity targetted by the artifact</param>
    public void ApplyEffects(EntityStats source, EntityStats target) {
        foreach (Pair<IAction, ActionTarget> action in actions) {
            if (action.second == ActionTarget.TARGET) action.first.Use(source, target);
            if (action.second == ActionTarget.SOURCE) action.first.Use(source, source);
        }
    }

    /// <summary>
    /// Applies energy cost and cast restrictions such as cooldown and max uses per turn
    /// </summary>
    /// <param name="source">The player entity that cast the artifact</param>
    protected void ApplyCosts(PlayerStats source) {
        source.UseEnergy(cost);
        --remainingUsesThisTurn;
        wasUsedSinceLastTurnStart = true;
	}

    /// <summary>
    /// Tells if the artifact can be cast by the source entity
    /// </summary>

    public bool CanUse(PlayerStats source) {
        return source.CurrentEnergy >= Cost && remainingCooldown == 0 && remainingUsesThisTurn > 0;
	}

    public void TurnStart() {
        if (wasUsedSinceLastTurnStart) {
            remainingCooldown = cooldown;
            wasUsedSinceLastTurnStart = false;
        }
        else if (remainingCooldown > 0)
            --remainingCooldown;
        remainingUsesThisTurn = maximumUsePerTurn;
	}

    public abstract bool CanTarget(Tile tile);
    public abstract void Launch(PlayerStats source, Tile tile, Animator animator);

	public int GetMaxDistance() { return distanceMax; }
	public int GetMinDistance() { return distanceMin; }

	/***********************/
	/*                     */
	/*  GETTERS | SETTERS  */
	/*                     */
	/***********************/
	public GameObject Prefab     { get => prefab;            set => prefab = value;            }
    public int Cost              { get => cost;              set => cost = value;              }
    public int DistanceMin       { get => distanceMin;       set => distanceMin = value;       }
    public int DistanceMax       { get => distanceMax;       set => distanceMax = value;       }
    public int MaximumUsePerTurn { get => maximumUsePerTurn; set => maximumUsePerTurn = value; }
    public int Cooldown          { get => cooldown;          set => cooldown = value;          }
    public float LootRate        { get => lootRate;          set => lootRate = value;          }
    public Vector2 Size             { get => size;             set => size = value;             }
}
