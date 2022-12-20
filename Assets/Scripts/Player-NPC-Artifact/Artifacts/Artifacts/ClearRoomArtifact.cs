using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearRoomArtifact : AoeArtifact
{
	protected override void InitValues() {

        title = "Baggare";
        description = "Ô toi faitnéant, gagne en un rien de temps.";
        effect = "Effets";
        effectDescription = "Gagne la baggare.";

        cost = 0;

		attackDuration = 1f;

		range = new CircleAttackTS(0, 0);
		area = new CircleTileSearch(1, int.MaxValue);


		maximumUsePerTurn = 0;
		cooldown = 0;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Enemy");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new DamageAction(source, target, 1000, 1000));
	}
}
