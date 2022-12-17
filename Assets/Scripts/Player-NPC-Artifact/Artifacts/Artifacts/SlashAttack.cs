using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class SlashAttack : SingleTargetArtifact
{
	protected override void InitValues() {
		attackDuration = 5f;

        title = "Griffe de Kameiko";
        description = "Enragement canin \n L’instinct remplace la raison \n se soumet à l'eko";
        effect = "Effets";
        effectDescription = "";

        cost = 3;

        //playerColor = Color.red;
        weapon = WeaponEnum.sword;

		range = new CircleAttackTS(1, 2);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(2, 3);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
	}
}
