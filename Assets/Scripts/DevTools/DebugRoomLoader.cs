using System.Collections;
using UnityEngine;

public class DebugRoomLoader : MonoBehaviour
{
    public Room prefab;
	public int layoutIndex;

	private void Start() {
		StartCoroutine(LoadRoom());
	}

	private IEnumerator LoadRoom() {
		RoomInfo roomInfo = new RoomInfo(prefab, layoutIndex);
		Room currentRoom = roomInfo.LoadRoom(direction => false, null);
        yield return currentRoom.GetComponent<PlayerDeploy>().DeployPlayer(FindAnyObjectByType<PlayerTurn>().transform, Direction.NULL);
		FindAnyObjectByType<TurnSystem>().CheckForCombatStart();
	}
}
