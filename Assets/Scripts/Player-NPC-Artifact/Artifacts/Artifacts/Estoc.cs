using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Estoc : SingleTargetArtifact
{
	protected override void InitValues() {
        cost = 2;

		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));


		range = new CircleAttackTS(1, 2);

		maximumUsePerTurn = 1;
		cooldown = 0;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
		ActionManager.AddToBottom(new ApplyStatusAction(target, new AttackDownStatus(1)));
	}
}
