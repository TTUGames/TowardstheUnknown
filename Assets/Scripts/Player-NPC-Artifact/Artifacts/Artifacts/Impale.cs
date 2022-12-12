using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impale : SingleTargetArtifact
{
	protected override void InitValues() {
		cost = 4;

		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));

		range = new LineTileSearch(1, 1); //Forme de la portée
		//area = new AreaInfo(<Portée min>, <Portée max>, AreaType.[CIRCLE|CROSS]); //Forme de l’AOE, uniquement pour les AoeArtifacts

		maximumUsePerTurn = 1;
		cooldown = 4;

   		size = new Vector2Int(4, 1); //PLACEHOLDER
   		lootRate = 0.01f; //PLACEHOLDER

		targets.Add("Enemy"); //Indique la cible (“Enemy” ou “Player”. Mettre deux lignes pour cibler les deux.
				//Pour un singletarget, définit ce qui est ciblable, pour une AoE, définit ce qui est affecté en tant que cible
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target) {
   	 ActionManager.AddToBottom(new DamageAction(source, target, 30, 70));
    }
}
