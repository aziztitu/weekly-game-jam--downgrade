using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIController : MonoBehaviour
{
    [Serializable]
    public class AiStats
    {
        [Range(0, 1)] public float baseAggression = 0.5f;

        [Range(0, 1)] public float smartness = 0.5f;

        public RangeFloat preferredDistFromTargetRange = new RangeFloat(15, 15);
        public float preferredDistFromTarget => preferredDistFromTargetRange.selected;

        public RangeFloat aggressionLimits = new RangeFloat(0, 1);

        public RangeFloat idleAggChangeRateRange = new RangeFloat(0.04f, 0.1f);
        public RangeFloat idleAggChangeThresholdRange = new RangeFloat(3, 6);
        public float idleAggChangeThreshold => idleAggChangeThresholdRange.selected;

        public RangeFloat activeAggChangeRateRange = new RangeFloat(0.04f, 0.1f);
        public RangeFloat activeAggChangeThresholdRange = new RangeFloat(3, 6);
        public float activeAggChangeThreshold => activeAggChangeThresholdRange.selected;
    }

    [Header("Stats")] public AiStats stats;
    public RangeFloat combatRadiusRange = new RangeFloat(5, 8);
    [Range(0, 1)] public float dodgeOutsideCombatProbability = 0.2f;
    public SimpleTimer dodgeOutsideCombatTimer = new SimpleTimer();

    [Header("Combat")]
    [Range(0, 1)] public float comboContinueProbability = 0.9f;
    public SimpleTimer combatActionTimer = new SimpleTimer();
    public SimpleTimer blockTimer = new SimpleTimer();

    [Header("Data")] [Range(0, 1)] public float curAggression = 0.5f;
    public float moveTargetDist = 15f;
    public bool moveTargetDistIsMax = true;
    public float stopDistance = 0.5f;
    private Vector3? moveTarget;

    [Header("Input")] public float moveLerpFactor = 10f;

    [Header("Misc")] public RangeFloat updateMoveTargetIntervalRange = new RangeFloat(2, 4);
    public float updateMoveTargetInterval => updateMoveTargetIntervalRange.selected;
    [SerializeField] [ReadOnly] private float timeSinceLastUpdateMoveTarget = float.MaxValue;

    private CharacterModel targetCharacter => characterModel?.lockedOnTarget?.GetComponent<CharacterModel>();
    private float distFromTarget => Vector3.Distance(transform.position, targetCharacter.transform.position);

    public CharacterModel characterModel { get; private set; }
    public MeleeTest characterMeleeController => characterModel.characterMeleeController;

    private bool isOutsidePrefferedRange => distFromTarget > stats.preferredDistFromTarget + stopDistance;
    private bool isOutsideCombatRange => distFromTarget > combatRadiusRange.max + stopDistance;

    private bool continueCombo = false;

    void Awake()
    {
        characterModel = GetComponent<CharacterModel>();
    }

    // Start is called before the first frame update
    void Start()
    {
        OnAiStatsUpdated();

        combatActionTimer.Expire();
        blockTimer.Expire();

        characterModel.characterAnimEventHandler.onComboContinueCheckStarted += () =>
        {
            continueCombo = Random.Range(0f, 1f) < comboContinueProbability;
        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, stats.preferredDistFromTargetRange.min);
        Gizmos.DrawWireSphere(transform.position, stats.preferredDistFromTargetRange.max);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatRadiusRange.min);
        Gizmos.DrawWireSphere(transform.position, combatRadiusRange.max);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (targetCharacter)
        {
            DebugExtension.DebugArrow(transform.position,
                (targetCharacter.transform.position - transform.position).normalized);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastUpdateMoveTarget += Time.deltaTime;
        if (timeSinceLastUpdateMoveTarget >= updateMoveTargetInterval)
        {
            UpdateMoveTarget();
        }

        dodgeOutsideCombatTimer.Update();
        combatActionTimer.Update();
        blockTimer.Update();

        var newInput = new CharacterModel.CharacterInput();

        if (isOutsidePrefferedRange || isOutsideCombatRange)
        {
            if (moveTarget.HasValue)
            {
                var toMoveTarget = moveTarget.Value - transform.position;
                if (toMoveTarget.magnitude > stopDistance)
                {
                    if (dodgeOutsideCombatTimer.expired && Random.Range(0f, 1f) < dodgeOutsideCombatProbability)
                    {
                        newInput.Dodge = true;
                        dodgeOutsideCombatTimer.Reset();
                    }
                    newInput.Move = transform.InverseTransformDirection(toMoveTarget).normalized;
                }
                else
                {
                    timeSinceLastUpdateMoveTarget += Time.deltaTime;
                }
            }

            newInput.Move = Vector3.Lerp(characterModel.characterInput.Move, newInput.Move,
                moveLerpFactor * Time.deltaTime);
        }
        else
        {
            PerformCombat(newInput);
        }

        characterModel.characterInput = newInput;
    }

    void PerformCombat(CharacterModel.CharacterInput newInput)
    {
        if (characterMeleeController.isAttackSequenceActive)
        {
            if (characterMeleeController.isLightAttackSequenceActive)
            {
                if (characterModel.characterAnimEventHandler.checkingComboContinue && continueCombo)
                {
                    newInput.LightAttack = true;
                    continueCombo = false;
                }
            }
            combatActionTimer.Reset();
        }
        else if (!blockTimer.expired)
        {
            newInput.IsBlocking = true;
            combatActionTimer.Reset();
        }
        else if (combatActionTimer.expired)
        {
            var attackDefendProbability = 0.5f;

            if (Random.Range(0f, 1f) < attackDefendProbability)
            {
                // Attack
                newInput.LightAttack = true;
            }
            else
            {
                // Defend
                newInput.IsBlocking = true;
                blockTimer.Reset();
            }

            combatActionTimer.Reset();
        }
    }

    void OnAiStatsUpdated()
    {
        stats.preferredDistFromTargetRange.SelectRandom();
        stats.idleAggChangeThresholdRange.SelectRandom();
        stats.activeAggChangeThresholdRange.SelectRandom();
    }

    void UpdateMoveTarget()
    {
        updateMoveTargetIntervalRange.SelectRandom();
        timeSinceLastUpdateMoveTarget = 0;

        if (isOutsidePrefferedRange)
        {
            moveTargetDist = stats.preferredDistFromTarget;
            moveTargetDistIsMax = true;
            ComputeNewMoveTarget();
            return;
        }

        if (isOutsideCombatRange)
        {
            if (curAggression >= 0.5 || curAggression >= Random.Range(0, 0.5f))
            {
                moveTargetDist = combatRadiusRange.GetRandom();
                moveTargetDistIsMax = true;
                ComputeNewMoveTarget();
                return;
            }

            moveTarget = null;
        }
    }

    void ComputeNewMoveTarget()
    {
        moveTarget = targetCharacter.transform.position +
                     (transform.position - targetCharacter.transform.position).normalized * moveTargetDist;
    }
}