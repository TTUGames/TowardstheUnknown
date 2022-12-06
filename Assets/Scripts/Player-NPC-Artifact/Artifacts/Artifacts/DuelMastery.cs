using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMastery : SingleTargetArtifact
{
	protected override void InitValues() {
		cost = 2;

		vfxInfos.Add(new VFXInfo(GetType().Name, VFXInfo.Target.SWORD));

		range = new LineTileSearch(1, 1);

		maximumUsePerTurn = 1;
		cooldown = 0;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 30, 30));
		ActionManager.AddToBottom(new ArmorAction(source, 30));
	}
}
