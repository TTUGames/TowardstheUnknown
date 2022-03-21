using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    [SerializeField] protected int cost;

    [SerializeField] protected int distanceMin;
    [SerializeField] protected int distanceMax;

    [SerializeField] protected int areaOfEffectMin;
    [SerializeField] protected int areaOfEffectMax;

    [SerializeField] protected int damageMin;
    [SerializeField] protected int damageMax;

    [SerializeField] protected int   maximumUsePerTurn;
    [SerializeField] protected int   cooldown;
    [SerializeField] protected float lootRate;

    [SerializeField] protected int sizeX;
    [SerializeField] protected int sizeY;








    /***********************/
    /*                     */
    /*  GETTERS | SETTERS  */
    /*                     */
    /***********************/

    public int Cost              { get => cost;              set => cost = value;              }
    public int DistanceMin       { get => distanceMin;       set => distanceMin = value;       }
    public int DistanceMax       { get => distanceMax;       set => distanceMax = value;       }
    public int AreaOfEffectMin   { get => areaOfEffectMin;   set => areaOfEffectMin = value;   }
    public int AreaOfEffectMax   { get => areaOfEffectMax;   set => areaOfEffectMax = value;   }
    public int DamageMin         { get => damageMin;         set => damageMin = value;         }
    public int DamageMax         { get => damageMax;         set => damageMax = value;         }
    public int MaximumUsePerTurn { get => maximumUsePerTurn; set => maximumUsePerTurn = value; }
    public int Cooldown          { get => cooldown;          set => cooldown = value;          }
    public int LootRate          { get => lootRate;          set => lootRate = value;          }
    public int SizeX             { get => sizeX;             set => sizeX = value;             }
    public int SizeY             { get => sizeY;             set => sizeY = value;             }
}
