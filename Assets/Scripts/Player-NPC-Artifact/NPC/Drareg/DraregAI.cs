using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DraregAttack))]
public class DraregAI : EnemyAI {
    private int firstPhaseLayout;
    private bool isInSecondPhase = false;

    [SerializeField] private int ultimateCooldown = 3;
    [SerializeField] private int currentUltimateCooldown = 3;

    [SerializeField] private GameObject phase1Model;
    [SerializeField] private GameObject phase2Model;

    [SerializeField] private Avatar phase2Avatar;

    protected override void Init() {
        firstPhaseLayout = Random.Range(0, 2);
        base.Init();
    }

    protected override void SetTargetting() {
        if (!isInSecondPhase) {
            switch (firstPhaseLayout) {
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
        else {
            targetting = new PlayerTargetting(1);
		}
    }

    protected override void SetAttackPatterns() {
        if (!isInSecondPhase) {
            switch (firstPhaseLayout) {
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
        else {
            attack.ClearPatterns();
            ((DraregAttack)attack).SetSpecialPattern(new DraregUltimateSuccess(), new DraregUltimateFail());

            attack.AddPattern(new DraregDragonStrikePattern());
            attack.AddPattern(new DraregKineticVortexPattern());
            attack.AddPattern(new DraregBlastPattern());
        }
    }

	public override void TurnUpdate() {
        if (!isInSecondPhase || currentUltimateCooldown != 0) base.TurnUpdate();
        else {
            if (!hasAttacked) {
                ((DraregAttack)attack).UseSpecialPattern(currentTarget);
                hasAttacked = true;
            }
            else {
                ActionManager.AddToBottom(new EndTurnAction());
            }
		}
	}

	public override void OnTurnStop() {
        if (isInSecondPhase) {
            if (currentUltimateCooldown == 0) currentUltimateCooldown = ultimateCooldown;
            else currentUltimateCooldown -= 1;
        }
    }

    public void SwitchToSecondPhase() {
        if (isInSecondPhase) throw new System.Exception("Drareg already is in second phase");
        ActionManager.AddToBottom(new DraregPhaseTransitionAction(this));

        isInSecondPhase = true;
        InitAI();
    }

    public void SwitchModel() {
        phase1Model.SetActive(false);
        phase2Model.SetActive(true);

        GetComponent<Animator>().avatar = phase2Avatar;
	}

    public bool IsInSecondPhase { get { return isInSecondPhase; } }
}
