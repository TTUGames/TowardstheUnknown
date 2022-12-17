using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impale : SingleTargetArtifact
{
	protected override void InitValues() {
		cost = 4;
        attackDuration = 2f;

		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.SWORD));

        title = "Empalame";
        description = "Lame infusée d’Eko\nPuisses-tu atteindre leurs coeurs\nQue ce coup soit fatal";
        effect = "Effets";
        effectDescription = "";

        range = new LineTileSearch(1, 1);

		maximumUsePerTurn = 1;
		cooldown = 4;

   		size = new Vector2Int(4, 1); //PLACEHOLDER
   		lootRate = 0.01f; //PLACEHOLDER

		targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target) {
   	    ActionManager.AddToBottom(new DamageAction(source, target, 30, 70));
    }
}
