using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecisionShoot : SingleTargetArtifact
{
    public PrecisionShoot() {
   	 //this.Prefab = (GameObject)Resources.Load("<Chemin du VFX depuis le dossier Resources>", typeof(GameObject));

   	 cost = 3;

   	 range = new AreaInfo(3, 5, AreaType.CIRCLE); //Forme de la portée

   	 maximumUsePerTurn = 2;
   	 cooldown = 0;

   	 size = new Vector2(1, 1); //PLACEHOLDER
   	 lootRate = 0.01f; //PLACEHOLDER

   	 targets.Add("Enemy"); //Indique la cible (“Enemy” ou “Player”. Mettre deux lignes pour cibler les deux.
			//Pour un singletarget, définit ce qui est ciblable, pour une AoE, définit ce qui est affecté en tant que cible
    }

    public override void ApplyEffects(PlayerStats source, EntityStats target) {
   	 ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
    }
}
