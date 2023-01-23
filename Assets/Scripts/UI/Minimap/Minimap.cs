using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private List<List<MinimapElement>> minimapElements;
    [SerializeField] int elementSize;

    private Vector2Int currentRoom;

    private Vector2Int TransformToDisplayPos(Vector2Int pos, Vector2Int mapSize) {
        return new Vector2Int(mapSize.x - 1 - pos.y, pos.x + 1 - mapSize.x) * elementSize;
	}

    public void SetMap(List<List<RoomInfo>> roomInfos) {
        Vector2Int mapSize = new Vector2Int(roomInfos.Count, roomInfos[0].Count);
        minimapElements = new List<List<MinimapElement>>();
        for (int x = 0; x < roomInfos.Count; ++x) {
            minimapElements.Add(new List<MinimapElement>());
            for(int y = 0; y < roomInfos[x].Count; ++y) {
                if (roomInfos[x][y] == null) minimapElements[x].Add(null);
                else {
                    minimapElements[x].Add(MinimapElement.InstantiateElement(transform, roomInfos[x][y].GetRoomType()));
                    minimapElements[x][y].transform.localPosition = (Vector2)TransformToDisplayPos(new Vector2Int(x, y), mapSize);
				}
			}
		}
	}

    public void SetCurrentRoom(Vector2Int position) {
        if (minimapElements[currentRoom.x][currentRoom.y] != null) minimapElements[currentRoom.x][currentRoom.y].SetCurrent(false);
        currentRoom = position;
        minimapElements[currentRoom.x][currentRoom.y].SetCurrent(true);
	}
}
