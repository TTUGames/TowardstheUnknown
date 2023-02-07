using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOArtifact", menuName = "Inventory/Artifact")]
public class SOArtifact : ScriptableObject
{
    [SerializeField] private GameObject prefab;

    [SerializeField] protected string title;
    [SerializeField] protected string description;
    [SerializeField] protected Sprite icon;

    [SerializeField] protected int cost;

    [SerializeField] protected int distanceMin;
    [SerializeField] protected int distanceMax;

    [SerializeField] protected int areaOfEffectMin;
    [SerializeField] protected int areaOfEffectMax;

    [SerializeField] protected int damageMin;
    [SerializeField] protected int damageMax;

    [SerializeField] protected int maximumUsePerTurn;
    [SerializeField] protected int cooldown;
    [SerializeField] protected Vector2 size;

    [SerializeField] protected List<string> targets = new List<string>();
    [SerializeField] protected PlayerStats source;
    [SerializeField] protected EntityStats target;

    [SerializeField] protected IArtifact artifact;



    /***********************/
    /*                     */
    /*  GETTERS | SETTERS  */
    /*                     */
    /***********************/
    public GameObject Prefab { get => prefab; set => prefab = value; }
    public string Title { get => title; set => title = value; }
    public string Description { get => description; set => description = value; }
    public Sprite Icon { get => icon; set => icon = value; }
    public int Cost { get => cost; set => cost = value; }
    public int DistanceMin { get => distanceMin; set => distanceMin = value; }
    public int DistanceMax { get => distanceMax; set => distanceMax = value; }
    public int AreaOfEffectMin { get => areaOfEffectMin; set => areaOfEffectMin = value; }
    public int AreaOfEffectMax { get => areaOfEffectMax; set => areaOfEffectMax = value; }
    public int DamageMin { get => damageMin; set => damageMin = value; }
    public int DamageMax { get => damageMax; set => damageMax = value; }
    public int MaximumUsePerTurn { get => maximumUsePerTurn; set => maximumUsePerTurn = value; }
    public int Cooldown { get => cooldown; set => cooldown = value; }
    public Vector2 Size { get => size; set => size = value; }
}
