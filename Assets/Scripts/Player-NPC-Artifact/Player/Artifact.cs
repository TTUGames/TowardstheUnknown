using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    [SerializeField] private int cost;

    [SerializeField] private int distanceMin;
    [SerializeField] private int distanceMax;

    [SerializeField] private int areaOfEffectMin;
    [SerializeField] private int areaOfEffectMax;

    [SerializeField] private int damageMin;
    [SerializeField] private int damageMax;

    [SerializeField] private int maximumUsePerTurn;
    [SerializeField] private int cooldown;
    [SerializeField] private int lootRate;

    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;








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
