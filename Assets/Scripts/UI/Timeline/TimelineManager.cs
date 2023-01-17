using UnityEngine;
using System.Collections;
using System.Linq;

public class TimelineManager : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating("DisplayEntity", 0f, 5f);
    }
    
    void DisplayEntity()
    {
        // Récupère tous les objets dans la scène
        var objects = FindObjectsOfType<GameObject>();

        // Filtre les objets qui ont soit le script PlayerStats soit le script EnemyStats
        var filteredObjects = objects.Where(obj => obj.GetComponent<PlayerStats>() != null || obj.GetComponent<EnemyStats>() != null);

        string list = "Nombre d'entités : " + filteredObjects.Count() + "\nVoici la liste des entités : ";
        // Ajoute le nom des objets à la liste
        for (int i = 0; i < filteredObjects.Count(); i++)
        {
            list += filteredObjects.ElementAt(i).name;
            if(i != filteredObjects.Count() -1)
            {
                list += ", ";
            }
        }
        // Affiche la liste dans la console
        Debug.Log(list);
    }
}
