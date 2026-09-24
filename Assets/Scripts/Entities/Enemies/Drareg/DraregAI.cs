using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(DraregAttack))]
public class DraregAI : EnemyAI
{
    [BoxGroup("Patterns"), Tooltip("One of them is picked randomly for the first phase")]
    [SerializeField] private List<EnemyPatternSet> firstPhaseLayouts = new List<EnemyPatternSet>();
    [BoxGroup("Patterns"), SerializeField] private EnemyPatternSet secondPhase = new EnemyPatternSet();
    [BoxGroup("Patterns"), SerializeField, Tooltip("Cast when the ultimate reaches the player")] private EnemyPatternData ultimateSuccess;
    [BoxGroup("Patterns"), SerializeField, Tooltip("Cast when the ultimate misses the player")] private EnemyPatternData ultimateFail;

    private bool isInSecondPhase = false;

    [SerializeField] private int ultimateCooldown = 2;
    [SerializeField] private int currentUltimateCooldown = 2;

    [SerializeField] private GameObject phase1Model;
    [SerializeField] private GameObject phase2Model;

    [SerializeField] private Avatar phase2Avatar;

    [BoxGroup("Phase transition"), SerializeField] private GameObject phaseTransitionVFX;
    [BoxGroup("Phase transition"), SerializeField] private GameObject chainsVFX;

    [SerializeField] private GameObject cataclysmIndicator1;
    [SerializeField] private GameObject cataclysmIndicator2;
    [SerializeField] private GameObject cataclysmIndicator3;
    private GameObject currentIndicator;
    [SerializeField] Animator animator;

    public GameObject PhaseTransitionVFX => phaseTransitionVFX;
    public GameObject ChainsVFX => chainsVFX;

    protected override EnemyPatternSet InitialPatternSet => firstPhaseLayouts[Random.Range(0, firstPhaseLayouts.Count)];

    protected override bool UsesPatternSet => false;

    /// <summary>
    /// In the second phase, casts the ultimate instead of moving and attacking when its cooldown is over
    /// </summary>
    protected override void PlayTurn()
    {
        if (!isInSecondPhase || currentUltimateCooldown != 0)
        {
            base.PlayTurn();
            return;
        }
        ((DraregAttack)attack).UseSpecialPattern(currentTarget);
        NextStep(EndTurn);
    }

    public void CataclysmIndicator()
    {
        if (currentIndicator != null) Destroy(currentIndicator);

        GameObject[] cataclysmIndicators = { cataclysmIndicator3, cataclysmIndicator1, cataclysmIndicator2 };
        if (currentUltimateCooldown >= 1 && currentUltimateCooldown <= 3)
        {
            currentIndicator = Instantiate(cataclysmIndicators[currentUltimateCooldown - 1], transform, false);
            currentIndicator.transform.localPosition = Vector3.zero;
        }
    }

    public override void OnTurnStop()
    {
        if (isInSecondPhase)
        {
            CataclysmIndicator();
            if (currentUltimateCooldown == 0) currentUltimateCooldown = ultimateCooldown;
            else currentUltimateCooldown -= 1;
        }
        base.OnTurnStop();
    }

    public void SwitchToSecondPhase()
    {
        if (isInSecondPhase) return;
        animator.Play("Chained");
        ActionManager.AddToBottom(new DraregPhaseTransitionAction(this));
        GetComponent<DraregStats>().maxMovementPoints = 2;
        isInSecondPhase = true;
        UsePatternSet(secondPhase);
        ((DraregAttack)attack).SetSpecialPattern(new EnemyPattern(ultimateSuccess), new EnemyPattern(ultimateFail));
    }

    public void SwitchModel()
    {
        phase1Model.SetActive(false);
        phase2Model.SetActive(true);
        GetComponent<Animator>().avatar = phase2Avatar;
    }

    public bool IsInSecondPhase { get { return isInSecondPhase; } }
}
