using UnityEngine;
using TMPro;

public class RoomInfosDisplay : MonoBehaviour
{
    // Référence à un objet TextMeshPro dans l'UI pour afficher le nom des prefabs.
    public TextMeshProUGUI displayText;

    public void UpdateText()
    {
        string roomName = "None";
        
        Room room = Object.FindObjectOfType<Room>();
        if (room != null)
            roomName = room.name.Replace("(Clone)", "");

        displayText.text = "<i>Carte : " + roomName;
    }
}
