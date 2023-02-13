using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Room))]
public abstract class RoomEnterEffect : MonoBehaviour
{
    public abstract void OnRoomEnter();
}
