using UnityEngine;
using TMPro;


public class RoomInfosDisplay : MonoBehaviour
{
    // Référence à un objet TextMeshPro dans l'UI pour afficher le nom des prefabs.
    public TextMeshProUGUI displayText;

    private string prefabRoomName = "";
    private string prefabLayoutName = "";    
    void Update()
    {
        // Récupère tous les prefabs du projet qui contiennent "Room" dans leur nom.
        Object[] prefabs = Object.FindObjectsOfType(typeof(GameObject));

        // Parcours tous les prefabs trouvés et affiche leur nom dans l'UI.
        prefabLayoutName = "None";
        foreach (GameObject prefab in prefabs)
        {
            if (prefab.name.Contains("Room") && prefab.name.Contains("Clone"))
                prefabRoomName = prefab.name.Replace("(Clone)", "");
            if (prefab.name.Contains("SpawnLayout") && !prefab.name.Contains("SpawnLayouts"))
                prefabLayoutName = prefab.name.Replace("SpawnLayout","");
        }
        displayText.text = "Carte : " + prefabRoomName + " | Layout : " + prefabLayoutName;
    }
}
