using System.Collections;
using System.Collections.Generic; //remove unused dependencies
using UnityEngine;

public class BasicDamage : SingleTargetArtifact
{
    protected override void InitValues()
    {
        attackDuration = 2f;
        vfxInfos.Add(new VFXInfo("VFX/00-Prefab/" + GetType().Name, VFXInfo.Target.GUN));

        title = "Taillade";
        description = "Que la brave lame \nInflige douleur certaine \nAux êtres né d'eko";
        effect = "Effets";
        effectDescription = "Occasionne <color=#e82a65>20</color> à <color=#e82a65>25</color> de dégats sur une cible, maximum 2 par tour.\nPortée d'attaque : 1";

        cost = 2;

        range = new CircleAttackTS(1, 1);

        maximumUsePerTurn = 2;
        cooldown = 0;

        slots = new List<Vector2Int>()
        {
            new Vector2Int(0,0),
        };

        lootRate = 0.01f;

        targets.Add("Enemy");
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        ActionManager.AddToBottom(new DamageAction(source, target, 30, 35));
    }
}
