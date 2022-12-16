using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicShield : SingleTargetArtifact
{
	protected override void InitValues() {
		attackDuration = 2f;
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        title = "Basic Shield";
        description = "La salle \n de muscu pour mieux se défendre";
        effect = "Damage";
        effectDescription = "Deals x damage to the target";

        cost = 2;

		range = new CircleAttackTS(0, 0);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ArmorAction(source, 10));
	}
}
