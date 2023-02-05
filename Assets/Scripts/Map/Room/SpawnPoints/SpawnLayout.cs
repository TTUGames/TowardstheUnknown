using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface SpawnLayout
{
	public void Spawn();

	/// <summary>
	/// Returns true if it activates on Enter, false if it activates on Clear
	/// </summary>
	public bool IsRoomReward();
}
