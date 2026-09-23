using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The room prefabs a random map is built from, by room type
/// </summary>
[CreateAssetMenu(fileName = "RoomSet", menuName = "TTU/Room Set")]
public class RoomSet : ScriptableObject
{
    public List<Room> spawnRooms = new List<Room>();
    [Tooltip("Each spawn layout of these rooms is a possible fight, picked by its difficulty")]
    public List<Room> combatRooms = new List<Room>();
    public List<Room> treasureRooms = new List<Room>();
    public List<Room> antechamberRooms = new List<Room>();
    public List<Room> bossRooms = new List<Room>();
}
