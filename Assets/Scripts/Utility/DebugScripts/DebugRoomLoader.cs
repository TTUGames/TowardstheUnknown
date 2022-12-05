using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		Room currentRoom = roomInfo.LoadRoom(false, false, false, false);
        yield return currentRoom.GetComponent<PlayerDeploy>().DeployPlayer(FindObjectOfType<PlayerTurn>().transform, Direction.NORTH);
		FindObjectOfType<TurnSystem>().CheckForCombatStart();
	}
}
