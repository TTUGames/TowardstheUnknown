using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public EntityStats entityStats;
    void Start()
    {
        textMeshPro.text = "Score : 0";
    }
}
