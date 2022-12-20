using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecisionShoot : SingleTargetArtifact
{
	protected override void InitValues() {
   	 	cost = 3;
        attackDuration = 2f;

        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        title = "Tir de précision";
        description = "Quand l’espoir s’affaiblit \nEt quand l’acier ne suffit plus \nQue parle la poudre";
        effect = "Effets";
        effectDescription = "Occasionne <color=#e82a65>20</color> à <color=#e82a65>30</color> de dégats sur une cible, maximum 2 par tour.\nPortée d'attaque : 3 à 5";

        weapon = WeaponEnum.gun;

        range = new CircleAttackTS(3, 5); //Forme de la portée

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(2, 1); //PLACEHOLDER
		lootRate = 0.01f; //PLACEHOLDER

		targets.Add("Enemy"); //Indique la cible (“Enemy” ou “Player”. Mettre deux lignes pour cibler les deux.
				//Pour un singletarget, définit ce qui est ciblable, pour une AoE, définit ce qui est affecté en tant que cible
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 20, 30));
    }
}
