using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impale : SingleTargetArtifact
{
    public Impale() {
   	 //this.Prefab = (GameObject)Resources.Load("<Chemin du VFX depuis le dossier Resources>", typeof(GameObject));

   	 cost = 4;

   	 range = new LineTileSearch(1, 1); //Forme de la portée
	 //area = new AreaInfo(<Portée min>, <Portée max>, AreaType.[CIRCLE|CROSS]); //Forme de l’AOE, uniquement pour les AoeArtifacts

   	 maximumUsePerTurn = 1;
   	 cooldown = 4;

   	 size = new Vector2(1, 1); //PLACEHOLDER
   	 lootRate = 0.01f; //PLACEHOLDER

   	 targets.Add("Enemy"); //Indique la cible (“Enemy” ou “Player”. Mettre deux lignes pour cibler les deux.
			//Pour un singletarget, définit ce qui est ciblable, pour une AoE, définit ce qui est affecté en tant que cible
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target) {
   	 ActionManager.AddToBottom(new DamageAction(source, target, 75, 125));
    }

    protected override void PlayAnimation(Tile sourceTile, Tile targetTile, Animator animator) {
        Vector3 VFXposition = sourceTile.transform.position;
        VFXposition.y += 2;
        ActionManager.AddToBottom(new PlayAnimationAction(animator, animStateName));

        if (Prefab != null)
            ActionManager.AddToBottom(new WaitForVFXEnd(GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity)));
    }
}
