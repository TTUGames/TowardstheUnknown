using UnityEngine;
using TMPro;

public class RoomInfosDisplay : MonoBehaviour
{
    // Displays the current room prefab's name
    public TextMeshProUGUI displayText;

    public void UpdateText()
    {
        string roomName = Room.currentRoom != null ? Room.currentRoom.name.Replace("(Clone)", "") : "None";
        displayText.text = "<i>Room : " + roomName;
    }
}
