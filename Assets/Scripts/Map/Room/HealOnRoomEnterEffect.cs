using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealOnRoomEnterEffect : RoomEnterEffect {
	[SerializeField] float healRatio = 1f;

	public override void OnRoomEnter() {
		PlayerStats player = FindObjectOfType<PlayerStats>();
		player.Heal(Mathf.CeilToInt((player.MaxHealth - player.CurrentHealth) * healRatio));
	}
}
