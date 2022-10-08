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

    [SerializeField] protected Vector2 size;

    protected List<IAction> actions = new List<IAction>();

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

    public void ApplyEffects(EntityStats source, EntityStats target) {
        foreach (IAction action in actions) action.Use(source, target);
    }

    protected void SpendEnergy(PlayerStats source) {
        source.UseEnergy(cost);
	}

    public abstract void Launch(PlayerStats source, RaycastHit hitTerrain, Animator animator);
	public abstract bool IsRaycastHitAccepted(RaycastHit hitTerrain);
	public int GetMaxDistance() { return distanceMax; }
	public int GetMinDistance() { return distanceMin; }

    public int GetCost() { return cost; }


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
