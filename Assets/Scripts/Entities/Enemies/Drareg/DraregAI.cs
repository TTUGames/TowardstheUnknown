using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(EnemyAttack))]
public class DraregAI : EnemyAI
{
    [BoxGroup("Patterns"), Tooltip("One of them is picked randomly for the first phase")]
    [SerializeField] private List<EnemyPatternSet> firstPhaseLayouts = new List<EnemyPatternSet>();
    [BoxGroup("Patterns"), SerializeField] private EnemyPatternSet secondPhase = new EnemyPatternSet();
    [BoxGroup("Patterns"), SerializeField, Tooltip("Cast when the ultimate reaches the player")] private EnemyPatternData ultimateSuccess;
    [BoxGroup("Patterns"), SerializeField, Tooltip("Cast when the ultimate misses the player")] private EnemyPatternData ultimateFail;

    [BoxGroup("Second phase"), SerializeField, Tooltip("Turns between two ultimates")] private int ultimateCooldown = 2;
    [BoxGroup("Second phase"), SerializeField, Tooltip("Turns before the first ultimate")] private int firstUltimateCooldown = 2;
    [BoxGroup("Second phase"), SerializeField, Tooltip("Shown over Drareg while the ultimate comes, by remaining turns: the first one for 1 turn")]
    private List<GameObject> ultimateCountdownIndicators = new List<GameObject>();
    [BoxGroup("Second phase"), SerializeField] private int secondPhaseMovementPoints = 2;

    [BoxGroup("Models"), SerializeField] private GameObject phase1Model;
    [BoxGroup("Models"), SerializeField] private GameObject phase2Model;
    [BoxGroup("Models"), SerializeField] private Avatar phase2Avatar;
    [BoxGroup("Phase transition"), SerializeField, Tooltip("Played as the chains bind Drareg")] private AnimationClip chainedClip;

    [BoxGroup("Phase transition"), SerializeField] private GameObject phaseTransitionVFX;
    [BoxGroup("Phase transition"), SerializeField] private GameObject chainsVFX;
    [BoxGroup("Phase transition"), SerializeField, InlineProperty, HideLabel] private DraregPhaseTransitionAction.Settings transition = new DraregPhaseTransitionAction.Settings();

    private bool isInSecondPhase = false;
    private int ultimateCountdown;
    private GameObject currentIndicator;
    private EnemyPattern ultimate, ultimateMiss;

    public GameObject PhaseTransitionVFX => phaseTransitionVFX;
    public GameObject ChainsVFX => chainsVFX;

    protected override EnemyPatternSet InitialPatternSet => firstPhaseLayouts[Random.Range(0, firstPhaseLayouts.Count)];

    protected override bool UsesPatternSet => false;

    public override IEnumerable<EnemyPatternData> AllPatterns
    {
        get
        {
            foreach (EnemyPatternSet layout in firstPhaseLayouts)
                foreach (EnemyPatternData pattern in layout.patterns) yield return pattern;
            foreach (EnemyPatternData pattern in secondPhase.patterns) yield return pattern;
            yield return ultimateSuccess;
            yield return ultimateFail;
        }
    }

    /// <summary>
    /// In the second phase, casts the ultimate instead of moving and attacking when its cooldown is over
    /// </summary>
    protected override async Awaitable PlaySteps()
    {
        if (!isInSecondPhase || ultimateCountdown != 0)
        {
            await base.PlaySteps();
            return;
        }
        attack.UsePattern(ultimate.CanTarget(attack.CurrentTile, currentTarget) ? ultimate : ultimateMiss, currentTarget);
        if (await WaitForActions()) EndTurn();
    }

    /// <summary>
    /// Shows the indicator of the turns remaining before the ultimate
    /// </summary>
    private void ShowUltimateCountdown()
    {
        if (currentIndicator != null) Destroy(currentIndicator);

        if (ultimateCountdown >= 1 && ultimateCountdown <= ultimateCountdownIndicators.Count)
        {
            currentIndicator = Instantiate(ultimateCountdownIndicators[ultimateCountdown - 1], transform, false);
            currentIndicator.transform.localPosition = Vector3.zero;
        }
    }

    public override void OnTurnStop()
    {
        if (isInSecondPhase)
        {
            ShowUltimateCountdown();
            if (ultimateCountdown == 0) ultimateCountdown = ultimateCooldown;
            else ultimateCountdown -= 1;
        }
        base.OnTurnStop();
    }

    public void SwitchToSecondPhase()
    {
        if (isInSecondPhase) return;
        GetComponent<EntityAnimator>().PlayAttack(chainedClip);
        ActionManager.AddToBottom(new DraregPhaseTransitionAction(this, transition));
        GetComponent<DraregStats>().maxMovementPoints = secondPhaseMovementPoints;
        isInSecondPhase = true;
        ultimateCountdown = firstUltimateCooldown;
        UsePatternSet(secondPhase);
        ultimate = new EnemyPattern(ultimateSuccess);
        ultimateMiss = new EnemyPattern(ultimateFail);
    }

    public void SwitchModel()
    {
        phase1Model.SetActive(false);
        phase2Model.SetActive(true);
        GetComponent<EntityAnimator>().SetAvatar(phase2Avatar);
    }

    public bool IsInSecondPhase { get { return isInSecondPhase; } }
}
