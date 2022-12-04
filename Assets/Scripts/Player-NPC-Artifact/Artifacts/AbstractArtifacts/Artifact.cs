using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Artifact : IArtifact
{
    protected GameObject prefab;
    protected string animStateName;
    protected float vfxDelay = 0;
    protected float attackDuration = 0;
    protected bool makeVFXFollowOrigin = true;

    protected int cost = 0;
    
    protected string title;
    protected string description;
    protected string effect;
    protected string effectDescription;
    
    protected Sprite skillBarIcon;
    protected Sprite inventoryIcon;

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
        Prefab = (GameObject)Resources.Load("VFX/00-Prefab/" + GetType().Name, typeof(GameObject));
        AnimStateName = GetType().Name;
        skillBarIcon = (Sprite)Resources.Load("Sprites/" + GetType().Name, typeof(Sprite));
        inventoryIcon = (Sprite)Resources.Load("Sprites/Inventory" + GetType().Name, typeof(Sprite));
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
        Debug.Log("Turn start");
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

        if (vfxDelay == 0) { //If there is no delay, play the vfx then adds the WaitForAttackAction
            GameObject vfx = InstantiateVFX(source, targetTile);
            ActionManager.AddToBottom(new WaitForAttackEndAction(attackDuration, source.gameObject, vfx));
        }
        else { //If there is a delay, adds the WaitForAttackAction, then starts the coroutine to launch the vfx
            WaitForAttackEndAction action = new WaitForAttackEndAction(attackDuration, source.gameObject, null);
            ActionManager.AddToBottom(action);

            if (Prefab != null) {
                source.GetComponent<TacticsAttack>().StartCoroutine(PlayVFXDelayed(vfxDelay, source, targetTile, action));
            }
        }
    }
   
    /// <summary>
    /// Coroutine waiting a delay before launching a vfx.
    /// </summary>
    /// <param name="delay">The delay in seconds before the artifact is cast</param>
    /// <param name="position">Origin of the vfx</param>
    /// <param name="rotation">Rotation of the vfx</param>
    /// <param name="action">The <c>WaitForAttackEndAction</c> of the artifact, supposed to destroy the vfx</param>
    /// <returns></returns>
    protected IEnumerator PlayVFXDelayed(float delay, PlayerAttack source, Tile targetTile, WaitForAttackEndAction action) {
        yield return new WaitForSeconds(delay);
        GameObject vfx = InstantiateVFX(source, targetTile);
        action.SetVFX(vfx);
	}

    /// <summary>
    /// Instantiates dthe artifact's VFX, following a point if makeVFXFollowOrigin is set to true.
    /// </summary>
    /// <param name="source">The player </param>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    private GameObject InstantiateVFX(PlayerAttack source, Tile targetTile) {
        if (Prefab == null) return null;
        Transform VFXorigin = GetVFXOrigin(source, targetTile);
        Vector3 VFXposition = VFXorigin.position;
        Vector3 VFXdirection = GetVFXOrigin(source, targetTile).transform.position - GetVFXTarget(source, targetTile);
        VFXdirection.y = 0;
        float VFXrotation = -Vector3.SignedAngle(VFXdirection, Vector3.forward, Vector3.up);
        GameObject vfx = GameObject.Instantiate(Prefab, VFXposition, Quaternion.Euler(0, VFXrotation, 0));
        if (makeVFXFollowOrigin) {
            vfx.transform.SetParent(VFXorigin);
            vfx.AddComponent<ConstantRotation>().SetRotation(new Vector3(0, VFXrotation, 0));
        }

        return vfx;
    }

    /// <summary>
    /// Gets the artifact's vfx origin
    /// </summary>
    /// <param name="playerAttack">The player using the artifact</param>
    /// <param name="targetTile">The tile targetted by the player</param>
    /// <returns></returns>
    protected abstract Transform GetVFXOrigin(PlayerAttack playerAttack, Tile targetTile);

    protected virtual Vector3 GetVFXTarget(PlayerAttack playerAttack, Tile targetTile) {
        return targetTile.transform.position;
	}


    /***********************/
    /*                     */
    /*  GETTERS | SETTERS  */
    /*                     */
    /***********************/


    public GameObject Prefab     { get => prefab;            set => prefab = value;            }
    public string AnimStateName  { get => animStateName;     set => animStateName = value;     }
    public int Cost              { get => cost;              set => cost = value;              }
    public string Title          { get => title;             set => title = value;             }
    public string Description    { get => description;       set => description = value;       }
    public string Effect         { get => effect;            set => effect = value;            }
    public string EffectDescription { get => effectDescription; set => effectDescription = value; }
    public Sprite SkillBarIcon   { get => skillBarIcon;      set => skillBarIcon = value;      }
    public Sprite InventoryIcon  { get => inventoryIcon;     set => inventoryIcon = value;     }
    
    public int MaximumUsePerTurn { get => maximumUsePerTurn; set => maximumUsePerTurn = value; }
    public int Cooldown          { get => cooldown;          set => cooldown = value;          }
    public float LootRate        { get => lootRate;          set => lootRate = value;          }
    public Vector2Int Size       { get => size;              set => size = value;              }

    public TileSearch GetRange() { return range;         }
    public Sprite     GetIcon()  { return skillBarIcon;  } //Need to be implemented
    public int        GetCost()  { return cost;          }
    public abstract List<Tile> GetTargets(Tile targetedTile);
}
