using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
	protected override void InitValues() {
		attackDuration = 5f;
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        title = "Basic Damage";
        description = "This is a very basic damage";
        effect = "Damage";
        effectDescription = "Deals x damage to the target";

        cost = 2;

		range = new CircleAttackTS(1, 1);

		maximumUsePerTurn = 2;
		cooldown = 2;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
	}
}
