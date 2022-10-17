using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Artifact : IArtifact
{
    private GameObject prefab;

    protected int cost = 0;

    protected int distanceMin = 0;
    protected int distanceMax = 0;
    protected AreaType rangeType = AreaType.CIRCLE;

    protected int   maximumUsePerTurn = 0;
    protected int   cooldown = 0;
    protected float lootRate = 0;

    protected int remainingUsesThisTurn;
    protected int remainingCooldown;
    protected bool wasUsedSinceLastTurnStart;

    protected Vector2 size = Vector2.one;
    protected List<string> targets = new List<string>();

    public abstract void ApplyEffects(PlayerStats source, EntityStats target);

    /// <summary>
    /// Applies energy cost and cast restrictions such as cooldown and max uses per turn
    /// </summary>
    /// <param name="source">The player entity that cast the artifact</param>
    protected void ApplyCosts(PlayerStats source) {
        source.UseEnergy(cost);
        --remainingUsesThisTurn;
        wasUsedSinceLastTurnStart = true;
	}

    public bool CanUse(PlayerStats source) {
        return source.CurrentEnergy >= Cost && remainingCooldown == 0 && (maximumUsePerTurn == 0 || remainingUsesThisTurn > 0);
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
    public AreaType GetRangeType() { return rangeType; }

    public abstract List<Tile> GetTargets();

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
