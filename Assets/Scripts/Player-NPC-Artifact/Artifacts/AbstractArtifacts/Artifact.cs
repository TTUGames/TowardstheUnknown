using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Artifact : IArtifact
{
    protected string animStateName;
    protected float attackDuration = 0;

    protected List<VFXInfo> vfxInfos = new List<VFXInfo>();

    protected int cost = 0;
    
    protected string title;
    protected string description;
    protected string effect;
    protected string effectDescription;
    
    protected Color      playerColor;
    protected WeaponEnum weapon; // -1 is pistol, 0 is hand, 1 is sword or 2 for both
    
    protected Sprite    skillBarIcon;
    protected Sprite    inventoryIcon;
    protected AudioClip sound;

    protected TileSearch range;

    protected int   maximumUsePerTurn = 0;
    protected int   cooldown = 0;
    protected float lootRate = 0;

    protected int remainingUsesThisTurn;
    protected int remainingCooldown;

    protected Vector2Int size = Vector2Int.one;
    protected List<string> targets = new List<string>();

    public Artifact() {
        SetValuesFromID();
        InitValues();
	}

    /// <summary>
    /// Initializes the artifact's specific values such as range, actions, ...
    /// </summary>
    protected abstract void InitValues();

    /// <summary>
    /// Initializes the artifact's values depending on its ID (VFX, animation, icons)
    /// </summary>
    protected void SetValuesFromID() {
        AnimStateName = GetType().Name;
        skillBarIcon  = (Sprite)   Resources.Load("Sprites/"          + GetType().Name, typeof(Sprite));
        inventoryIcon = (Sprite)   Resources.Load("Sprites/Inventory" + GetType().Name, typeof(Sprite));
        sound         = (AudioClip)Resources.Load("SFX/"              + GetType().Name, typeof(AudioClip));
    }


    /// <summary>
    /// Applies energy cost and cast restrictions such as cooldown and max uses per turn
    /// </summary>
    /// <param name="source">The player entity that cast the artifact</param>
    protected void ApplyCosts(PlayerStats source) {
        source.UseEnergy(cost);
        --remainingUsesThisTurn;
        if (remainingUsesThisTurn == 0 && remainingCooldown == 0)
            remainingCooldown = cooldown;
        
        GameObject.FindGameObjectWithTag("UI").transform.GetChild(0).Find("Skills").gameObject.GetComponent<UISkillsBar>().UpdateSkillBar(); //Refresh the UISkills after the attack is done
	}

    public bool CanUse(PlayerStats source) {
        return source.CurrentEnergy >= Cost && remainingCooldown == 0 && (maximumUsePerTurn == 0 || remainingUsesThisTurn > 0);
	}

    public void TurnStart() {
        if (remainingCooldown > 0)
            --remainingCooldown;
        remainingUsesThisTurn = maximumUsePerTurn;
	}

    public abstract bool CanTarget(Tile tile);
    public abstract void Launch(PlayerAttack source, Tile tile);

    /// <summary>
    /// Applies the artifacts' effects
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    protected abstract void ApplyEffects(PlayerStats source, EntityStats target);

    /// <summary>
    /// Plays the artifacts animation and vfx
    /// </summary>
    /// <param name="sourceTile"></param>
    /// <param name="targetTile"></param>
    /// <param name="animator"></param>
    protected virtual void PlayAnimation(Tile sourceTile, Tile targetTile, PlayerAttack source) {
        float modelRotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
        source.transform.rotation = Quaternion.Euler(0, modelRotation, 0);

        if (source.GetComponent<Animator>() != null) source.GetComponent<Animator>().Play(animStateName);

        WaitForAttackEndAction action = new WaitForAttackEndAction(attackDuration, source.gameObject, null);
        ActionManager.AddToBottom(action);

        foreach (VFXInfo vfxInfo in vfxInfos) {
            vfxInfo.Play(action, source.gameObject, targetTile);
		}
    }


    /***********************/
    /*                     */
    /*  GETTERS | SETTERS  */
    /*                     */
    /***********************/

    public string AnimStateName     { get => animStateName;     set => animStateName = value;     }
    public int Cost                 { get => cost;              set => cost = value;              }
    public string Title             { get => title;             set => title = value;             }
    public string Description       { get => description;       set => description = value;       }
    public string Effect            { get => effect;            set => effect = value;            }
    public string EffectDescription { get => effectDescription; set => effectDescription = value; }
    public Color PlayerColor        {                           set => playerColor = value;       }
    public WeaponEnum Weapon        {                           set => weapon = value;            }
    public Sprite SkillBarIcon      { get => skillBarIcon;      set => skillBarIcon = value;      }
    public Sprite InventoryIcon     { get => inventoryIcon;     set => inventoryIcon = value;     }
    public int MaximumUsePerTurn    { get => maximumUsePerTurn; set => maximumUsePerTurn = value; }
    public int Cooldown             { get => cooldown;          set => cooldown = value;          }
    public float LootRate           { get => lootRate;          set => lootRate = value;          }
    public Vector2Int Size          { get => size;              set => size = value;              }

    public TileSearch GetRange()  { return range;         }
    public Color      GetColor()  { return playerColor;   }
    public WeaponEnum GetWeapon() { return weapon;        }
    public Sprite     GetIcon()   { return skillBarIcon;  }
    public AudioClip  GetSound()  { return sound;         }
    public int        GetCost()   { return cost;          }
    public abstract List<Tile> GetTargets(Tile targetedTile);
}
