using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugRoomLoader : MonoBehaviour
{
    public Room prefab;
	[SerializeField] private List<GameObject> lTilePossible;
	public int layoutIndex;

	private void Start() {
		StartCoroutine(LoadRoom());
	}

	private IEnumerator LoadRoom() {
		RoomInfo roomInfo = new RoomInfo(prefab, layoutIndex);
		Room currentRoom = roomInfo.LoadRoom(false, false, false, false);
        ReloadTilesWithRandomPrefab();
        yield return currentRoom.GetComponent<PlayerDeploy>().DeployPlayer(FindObjectOfType<PlayerTurn>().transform, Direction.NORTH);
		FindObjectOfType<TurnSystem>().CheckForCombatStart();
	}

    private void ReloadTilesWithRandomPrefab()
    {
        GameObject.FindGameObjectsWithTag("Tile").ToList().ForEach(tile =>
        {
            int randomIndex = Random.Range(0, lTilePossible.Count);
            tile.GetComponent<MeshFilter>().sharedMesh = lTilePossible[randomIndex].GetComponent<MeshFilter>().sharedMesh;
        });
    }
}
