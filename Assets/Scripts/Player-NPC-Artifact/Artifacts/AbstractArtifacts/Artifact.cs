using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Artifact : MonoBehaviour, IArtifact
{
    private GameObject prefab;

    protected int cost = 0;
    
    protected string title;
    protected string description;
    protected Sprite icon;

    protected TileSearch range;

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
    

	/***********************/
	/*                     */
	/*  GETTERS | SETTERS  */
	/*                     */
	/***********************/
    
    
	public GameObject Prefab     { get => prefab;            set => prefab = value;            }
    public int Cost              { get => cost;              set => cost = value;              }
    public string Title          { get => title;             set => title = value;             }
    public string Description    { get => description;       set => description = value;       }
    public Sprite Icon           { get => icon;              set => icon = value;              }
    public int MaximumUsePerTurn { get => maximumUsePerTurn; set => maximumUsePerTurn = value; }
    public int Cooldown          { get => cooldown;          set => cooldown = value;          }
    public float LootRate        { get => lootRate;          set => lootRate = value;          }
    public Vector2 Size          { get => size;              set => size = value;              }

    public TileSearch GetRange() { return range; }
    public abstract List<Tile> GetTargets(Tile targetedTile);
}
