using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
	protected override void InitValues() {
		attackDuration = 2f;
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        title = "Taillade";
        description = "Que la brave lame \nInflige douleur certaine \nAux �tres n� d'eko";
        effect = "Effets";
        effectDescription = "Occasionne <color=#e82a65>20</color> � <color=#e82a65>25</color> de d�gats sur une cible, maximum 2 par tour.\nPort�e d'attaque : 1";

        cost = 2;

		range = new CircleAttackTS(1, 1);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 25));
	}
}
