using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraregAI : EnemyAI {
    private int firstPhaseLayout;
    private bool isInSecondPhase = false;

    protected override void Init() {
        firstPhaseLayout = Random.Range(0, 2);
        base.Init();
    }

    protected override void SetTargetting() {
        switch(firstPhaseLayout) {
            case 0:
                Debug.Log("DRAREG HAS SET 1");
                targetting = new PlayerTargetting(1);
                break;
            case 1:
                Debug.Log("DRAREG HAS SET 2");
                targetting = new PlayerTargetting(2);
                break;
        }
    }

    protected override void SetAttackPatterns() {
        switch(firstPhaseLayout) {
            case 0:
                attack.AddPattern(new DraregBasicDamagePattern());
                attack.AddPattern(new DraregPrecisionShootPattern());
                attack.AddPattern(new DraregHauntingPattern());
                break;
            case 1:
                attack.AddPattern(new DraregPrecisionShootPattern());
                attack.AddPattern(new DraregShockWavePattern());
                attack.AddPattern(new DraregHauntingPattern());
                break;
        }
    }

	public override void TurnUpdate() {
        if (!isInSecondPhase) base.TurnUpdate();
        else {
            if (ActionManager.IsBusy) return;
            Debug.Log("Drareg is WIP");
            ActionManager.AddToBottom(new EndTurnAction());
		}
	}

	public void SwitchToSecondPhase() {
        if (isInSecondPhase) throw new System.Exception("Drareg already is in second phase");
        Debug.Log("DRAREG IS ANGRY");
        ActionManager.AddToBottom(new DraregPhaseTransitionAction(this));
        isInSecondPhase = true;
	}

    public bool IsInSecondPhase { get { return isInSecondPhase; } }
}
