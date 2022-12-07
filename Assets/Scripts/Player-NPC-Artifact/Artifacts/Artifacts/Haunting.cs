using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Haunting : SingleTargetArtifact
{
	protected override void InitValues() {
		cost = 3;

		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.LEFTHAND));

		range = new CircleAttackTS(1, 4);

		maximumUsePerTurn = 1;
		cooldown = 3;

		size = new Vector2Int(3, 2);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(2)));
		ActionManager.AddToBottom(new DamageAction(source, target, 10, 20));
	}
}
