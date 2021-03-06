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

    [Serializable]
    public class CombatModifier
    {
        public float probability;
        public RangeFloat delta;
        public float smartnessProbFactor;
        public float smartnessFactor;

        public CombatModifier(float prob, RangeFloat delta, float smartnessFactor = 0)
        {
            probability = prob;
            this.delta = delta;
            this.smartnessFactor = smartnessFactor;
        }

        public float ProcessDelta(float smartnessEffect)
        {
            return (HelperUtilities.TestProbability(probability + (smartnessProbFactor * smartnessEffect))
                       ? delta.GetRandom()
                       : 0) + (smartnessFactor * smartnessEffect);
        }
    }

    [Header("Stats")] public AiStats stats;
    public RangeFloat combatRadiusRange = new RangeFloat(5, 8);
    [Range(0, 1)] public float dodgeOutsideCombatProbability = 0.2f;
    [Range(0, 1)] public float dodgeOutsideCombatSmartnessFactor = 0.2f;
    public SimpleTimer dodgeOutsideCombatTimer = new SimpleTimer();
    [Range(0, 0.5f)] public float smartnessDeviation = 0.2f;

    [Header("Combat")] [Range(0, 1)] public float attackDefendBaseProbability = 0.5f;

    [Range(0, 1)] public float comboContinueProbability = 0.9f;
    [Range(0, 1)] public float comboContinueSmartnessFactor = 0.1f;
    public RangeInt opponentParrySpamThreshold = new RangeInt(4, 8);
    public SimpleTimer combatActionTimer = new SimpleTimer();
    public float combatActionTimerSmartnessFactor = 0.5f;

    public SimpleTimer blockTimer = new SimpleTimer();
    public SimpleTimer dodgeTimer = new SimpleTimer();
    public SimpleTimer postAttackTimer = new SimpleTimer();
    public SimpleTimer postOpponentAttackTimer = new SimpleTimer();

    [Header("Attack Combat Modifiers")] public float justAttackedWindow = 0.5f;
    public CombatModifier attackAfterDodgeModifier = new CombatModifier(0.5f, new RangeFloat(0, 0.1f));

    public CombatModifier attackAfterOpponentStrikesModifier = new CombatModifier(0.3f, new RangeFloat(0, 0.1f));
    public CombatModifier attackIfOpponentStunnedModifier = new CombatModifier(0.9f, new RangeFloat(0.2f, 0.4f));
    public CombatModifier attackOnIncomingHeavyAttackModifier = new CombatModifier(0.65f, new RangeFloat(0, 0.2f));

    [Range(0, 1)] public float lightHeavyBaseProbability = 0.8f;
    public CombatModifier heavyIfOppenentShieldsModifier = new CombatModifier(0.3f, new RangeFloat(0, 0.1f));
    public CombatModifier heavyIfOppenentParrySpamModifier = new CombatModifier(0.75f, new RangeFloat(0, 0.6f));
    public CombatModifier lightOnIncomingHeavyAttackModifier = new CombatModifier(0.8f, new RangeFloat(0f, 0.2f));

    [Header("Defend Combat Modifiers")]
    public CombatModifier defendAfterAttackModifier = new CombatModifier(0.5f, new RangeFloat(0, 0.1f));

    public CombatModifier defendOnIncomingLightAttackModifier = new CombatModifier(0.2f, new RangeFloat(0, 0.1f));
    public CombatModifier defendOnIncomingHeavyAttackModifier = new CombatModifier(0.5f, new RangeFloat(0, 0.2f));

    [Range(0, 1)] public float blockDodgeBaseProbability = 0.7f;
    public CombatModifier blockAfterAttackModifier = new CombatModifier(0.5f, new RangeFloat(0, 0.1f));
    public CombatModifier blockOnIncomingAttackModifier = new CombatModifier(0.6f, new RangeFloat(0, 0.1f));
    [Range(0, 1)] public float parryOnIncomingAttackProbability = 0.1f;
    public CombatModifier dodgeOnIncomingHeavyAttackModifier = new CombatModifier(0.8f, new RangeFloat(0.2f, 0.5f));

    public float justDodgedWindow = 0.5f;
    public CombatModifier dodgeRandomModifier = new CombatModifier(0.1f, new RangeFloat(0, 0.2f));

    [Header("Data")] [Range(0, 1)] public float curAggression = 0.5f;
    public float moveTargetDist = 15f;
    public bool moveTargetDistIsMax = true;
    public float stopDistance = 0.5f;
    private Vector3? moveTarget;

    [Header("Input")] public float moveLerpFactor = 10f;

    [Header("Misc")] public RangeFloat updateMoveTargetIntervalRange = new RangeFloat(2, 4);
    public float updateMoveTargetInterval => updateMoveTargetIntervalRange.selected;
    [SerializeField] [ReadOnly] private float timeSinceLastUpdateMoveTarget = float.MaxValue;
    public bool drawGizmos = true;

    private CharacterModel targetCharacter => characterModel?.lockedOnTarget?.GetComponent<CharacterModel>();
    private float distFromTarget => Vector3.Distance(transform.position, targetCharacter.transform.position);

    public CharacterModel characterModel { get; private set; }
    public MeleeTest characterMeleeController => characterModel.characterMeleeController;

    private bool isOutsidePrefferedRange => distFromTarget > stats.preferredDistFromTarget + stopDistance;
    private bool isOutsideCombatRange => distFromTarget > combatRadiusRange.max + stopDistance;

    private bool continueCombo = false;

    private bool opponentIsShielding => targetCharacter.characterMeleeController.isShielding;

    private bool opponentIsSpammingParry => targetCharacter.characterMeleeController.continuousParryAttempts >=
                                            opponentParrySpamThreshold.selected;

    private bool justFinishedAttacking => !postAttackTimer.expired;
    private bool incomingAttackDetected => targetCharacter.characterMeleeController.isAttackSequenceActive;
    private bool incomingLightAttackDetected => targetCharacter.characterMeleeController.isLightAttackSequenceActive;
    private bool incomingHeavyAttackDetected => targetCharacter.characterMeleeController.isHeavyAttackSequenceActive;
    private bool justDodged => dodgeTimer.expired && dodgeTimer.timeSinceExpiry < justDodgedWindow;
    private bool opponentAttackJustEnded => !postOpponentAttackTimer.expired;
    private bool isOpponentStunned => targetCharacter.characterMeleeController.isInHitState;

    private float smartnessEffect => HelperUtilities.Remap(curSmartness, 0, 1, -1f, 1f);

    private float curSmartness = 0;
    private RangeFloat originalCombatActionDurationRange;

    void Awake()
    {
        characterModel = GetComponent<CharacterModel>();

        originalCombatActionDurationRange = new RangeFloat(combatActionTimer.durationRange.min, combatActionTimer.durationRange.max);
    }

    // Start is called before the first frame update
    void Start()
    {
        OnAiStatsUpdated();

        combatActionTimer.Expire();
        blockTimer.Expire();
        dodgeTimer.Expire();
        postAttackTimer.Expire();
        postOpponentAttackTimer.Expire();

        characterModel.characterAnimEventHandler.onMeleeAttackSequenceEnded += () => { postAttackTimer.Reset(); };

        characterModel.characterAnimEventHandler.onComboContinueCheckStarted += () =>
        {
            continueCombo =
                TestProbability(comboContinueProbability + (comboContinueSmartnessFactor * smartnessEffect));
        };

        characterModel.onInitialized += () =>
        {
            targetCharacter.characterAnimEventHandler.onMeleeAttackSequenceEnded +=
                () => { postOpponentAttackTimer.Reset(); };

            CalculateCurSmartness();
        };
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }

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
        dodgeTimer.Update();
        postAttackTimer.Update();
        postOpponentAttackTimer.Update();


        if (Time.timeScale < 0.1f || BattleManager.Instance.roundOver)
        {
            characterModel.characterInput = new CharacterModel.CharacterInput();
            return;
        }

        var newInput = new CharacterModel.CharacterInput();

        if (isOutsidePrefferedRange || isOutsideCombatRange)
        {
            if (moveTarget.HasValue)
            {
                var toMoveTarget = moveTarget.Value - transform.position;
                if (toMoveTarget.magnitude > stopDistance)
                {
                    if (dodgeOutsideCombatTimer.expired && TestProbability(
                            dodgeOutsideCombatProbability + (dodgeOutsideCombatSmartnessFactor * smartnessEffect)))
                    {
                        newInput.Move = new Vector3(0, 0, 1);
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

    void CalculateCurSmartness()
    {
        curSmartness = Mathf.Clamp01(stats.smartness + Random.Range(-smartnessDeviation, smartnessDeviation));

        combatActionTimer.durationRange.min = originalCombatActionDurationRange.min + combatActionTimerSmartnessFactor * smartnessEffect;
        combatActionTimer.durationRange.max = originalCombatActionDurationRange.max + combatActionTimerSmartnessFactor * smartnessEffect;
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
        else if (characterModel.characterMovementController.isDodging && !dodgeTimer.expired)
        {
            // No Action Required
        }
        else if (characterModel.characterMeleeController.isShielding && !blockTimer.expired &&
                 !incomingHeavyAttackDetected)
        {
            newInput.IsBlocking = true;
            combatActionTimer.Reset();
        }
        else if (combatActionTimer.expired || incomingAttackDetected)
        {
            var attackDefendProbability = ProcessAttackDefendProbability();
            if (TestProbability(attackDefendProbability))
            {
                // Attack
                var lightHeavyProbability = lightHeavyBaseProbability;

                if (incomingHeavyAttackDetected)
                {
                    lightHeavyProbability += lightOnIncomingHeavyAttackModifier.ProcessDelta(smartnessEffect);
                }

                if (opponentIsShielding)
                {
                    lightHeavyProbability -= heavyIfOppenentShieldsModifier.ProcessDelta(smartnessEffect);
                }

                if (opponentIsSpammingParry)
                {
                    lightHeavyProbability -= heavyIfOppenentParrySpamModifier.ProcessDelta(smartnessEffect);
                }

                if (TestProbability(lightHeavyProbability))
                {
                    newInput.LightAttack = true;
                }
                else
                {
                    newInput.HeavyAttack = true;
                }
            }
            else
            {
                // Defend
                var blockDodgeProbability = blockDodgeBaseProbability;

                if (justFinishedAttacking)
                {
                    blockDodgeProbability += blockAfterAttackModifier.ProcessDelta(
                        !characterModel.characterMeleeController.isInHitState
                            ? smartnessEffect
                            : 0);
                }

                if (incomingAttackDetected)
                {
                    blockDodgeProbability += blockOnIncomingAttackModifier.ProcessDelta(smartnessEffect);
                }

                if (incomingHeavyAttackDetected)
                {
                    blockDodgeProbability -= dodgeOnIncomingHeavyAttackModifier.ProcessDelta(smartnessEffect);
                }

                blockDodgeProbability -= dodgeRandomModifier.ProcessDelta(smartnessEffect);

                if (TestProbability(blockDodgeProbability))
                {
                    if (!incomingAttackDetected || TestProbability(parryOnIncomingAttackProbability))
                    {
                        newInput.AttemptParry = true;
                    }

                    newInput.IsBlocking = true;
                    blockTimer.Reset();
                }
                else
                {
                    newInput.Move = Random.insideUnitSphere;
                    newInput.Move.y = 0;
                    newInput.Move.Normalize();

                    newInput.Dodge = true;
                    dodgeTimer.Reset();
                }
            }

            combatActionTimer.Reset();
        }
    }

    float ProcessAttackDefendProbability()
    {
        var attackDefendProbability = 0.4f + (smartnessEffect * 0.15f);

        // Attack Modifiers

        if (justDodged)
        {
            attackDefendProbability += attackAfterDodgeModifier.ProcessDelta(smartnessEffect);
        }

        if (opponentAttackJustEnded)
        {
            attackDefendProbability +=
                attackAfterOpponentStrikesModifier.ProcessDelta(characterModel.characterMeleeController.isInHitState
                    ? smartnessEffect
                    : 0);
        }

        if (isOpponentStunned)
        {
            attackDefendProbability += attackIfOpponentStunnedModifier.ProcessDelta(smartnessEffect);
        }

        if (incomingHeavyAttackDetected && curSmartness > 0.75)
        {
            attackDefendProbability += attackOnIncomingHeavyAttackModifier.ProcessDelta(smartnessEffect);
        }

        // Defend Modifiers

        if (justFinishedAttacking)
        {
            attackDefendProbability -= defendAfterAttackModifier.ProcessDelta(
                !characterModel.characterMeleeController.isInHitState
                    ? smartnessEffect
                    : 0);
        }

        if (incomingLightAttackDetected)
        {
            attackDefendProbability -= defendOnIncomingLightAttackModifier.ProcessDelta(smartnessEffect);
        }

        if (incomingHeavyAttackDetected)
        {
            attackDefendProbability -= defendOnIncomingHeavyAttackModifier.ProcessDelta(smartnessEffect);
        }

        return attackDefendProbability;
    }

    bool TestProbability(float probability)
    {
        return HelperUtilities.TestProbability(probability);
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