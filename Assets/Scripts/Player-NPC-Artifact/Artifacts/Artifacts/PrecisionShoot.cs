using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecisionShoot : SingleTargetArtifact
{
	protected override void InitValues() {
   	 	cost = 3;

		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

   	 	range = new CircleAttackTS(3, 5); //Forme de la portée

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(1, 1); //PLACEHOLDER
		lootRate = 0.01f; //PLACEHOLDER

		targets.Add("Enemy"); //Indique la cible (“Enemy” ou “Player”. Mettre deux lignes pour cibler les deux.
				//Pour un singletarget, définit ce qui est ciblable, pour une AoE, définit ce qui est affecté en tant que cible
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
    }
}
