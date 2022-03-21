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

    [SerializeField] private int MaximumUsePerTurn;
    [SerializeField] private int cooldown;
    [SerializeField] private int lootRate;

    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
