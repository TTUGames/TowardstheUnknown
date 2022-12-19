using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicShield : SingleTargetArtifact
{
	protected override void InitValues() {
		attackDuration = 2f;
		vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        title = "Armure";
        description = "Vie, Désir, Volonté \nDans la faille, rien n’est éternel \nJuste un instant de plus";
        effect = "Effets";
        effectDescription = "Donne un bouclier temporaire de <color=#e82a65>10</color> sur une cible, maximum 2 par tour.\nPortée d'attaque : 0";

        cost = 2;

		range = new CircleAttackTS(0, 0);

		maximumUsePerTurn = 2;
		cooldown = 0;

		size = new Vector2Int(1, 1);
		lootRate = 0.01f;

		targets.Add("Player");
	}

	protected override void ApplyEffects(PlayerStats source, EntityStats target) {
		ActionManager.AddToBottom(new ArmorAction(source, 10));
	}
}
