using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoBomb : AoeArtifact
{
	protected override void InitValues() {
		attackDuration = 5.5f;

		vfxInfos.Add(new VFXInfo(GetType().Name, VFXInfo.Target.TARGETTILE, 2, Vector3.up * 0.5f));

		cost = 3;

        playerColor = Color.yellow;
        weapon = -1;


        range = new CircleAttackTS(1, 5); //Forme de la portée
		area = new CircleTileSearch(0, 2); //Forme de l’AOE, uniquement pour les AoeArtifacts

		maximumUsePerTurn = 1;
		cooldown = 2;

		size = new Vector2Int(1, 1);
		lootRate = 0f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target)
	{
        
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 40));
	}
}
