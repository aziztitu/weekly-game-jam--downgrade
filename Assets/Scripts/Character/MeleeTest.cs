using System.Collections;
using System.Collections.Generic;
using BasicTools.ButtonInspector;
using JetBrains.Annotations;
using UnityEngine;

public class MeleeTest : MonoBehaviour
{
    Animator anim => characterModel.animator;

    private CharacterModel characterModel;
    public Transform weaponHolder;
    public Transform shieldHolder;

    public Weapon currentWeapon { get; private set; }
    Collider weaponCollider => currentWeapon?.weaponCollider;

    public GameObject currentShield { get; private set; }
    public float shieldingAngle = 0;
    public float parryWindow = 0.5f;
    public SimpleTimer parryTimer = new SimpleTimer(1);
    public float continuousParryThreshold = 1;
    public SimpleTimer hitTimer = new SimpleTimer(1);
    public SimpleTimer stunnedTimer = new SimpleTimer(1.5f);
    public float parriedEffectSpeed = 3;
    public float parriedEffectDuration = 0.4f;

    [Header("Dash")] public float defaultDashSpeed;

    [Header("Debug")] public Vector3 attackerPosition;

    [Button("Try Shield", "TryShield")] [SerializeField]
    private bool _btnTryShield;

    [ReadOnly] public SimpleTimer shieldTimer = new SimpleTimer(0);

    public bool attemptingToShield =>
        !isAttackSequenceActive && characterModel.characterInput.IsBlocking;

    private bool wasAttemptingToShield = false;
    public bool isShielding => currentShield && !isInHitState && attemptingToShield;
    public bool isInHitState = false;
    public bool isInParryWindow => parryTimer.elapsedTime < parryWindow;
    [HideInInspector] public bool disableParry = false;

    public bool isAttackSequenceActive { get; private set; } = false;
    public bool isLightAttackSequenceActive => isAttackSequenceActive && anim.GetInteger("AttackMode") == 0;
    public bool isHeavyAttackSequenceActive => isAttackSequenceActive && anim.GetInteger("AttackMode") == 1;

    public float curDamageMultiplier { get; private set; } = 1;

    public bool comboContinued { get; private set; } = false;

    public int continuousParryAttempts { get; private set; } = 0;

    private void Awake()
    {
        characterModel = GetComponent<CharacterModel>();
    }

    // Start is called before the first frame update
    void Start()
    {
        hitTimer.Expire();

        OnWeaponSwitched();

        characterModel.characterAnimEventHandler.onMeleeAttackSequenceStarted +=
            () =>
            {
                this.isAttackSequenceActive = true;
                anim.ResetTrigger("CancelCombo");
            };
        characterModel.characterAnimEventHandler.onMeleeAttackSequenceEnded +=
            () =>
            {
                this.isAttackSequenceActive = false;
                UpdateWeaponCollider(false);
            };

        characterModel.characterAnimEventHandler.onMeleeAttackStarted += (int comboIndex, float damageMultiplier) =>
        {
            if (!isAttackSequenceActive)
            {
                return;
            }

            UpdateWeaponCollider(true);
            curDamageMultiplier = damageMultiplier;
        };
        characterModel.characterAnimEventHandler.onMeleeAttackEnded += () =>
        {
            UpdateWeaponCollider(false);
            characterModel.characterMovementController.StopDashing();
        };
        characterModel.characterAnimEventHandler.onComboContinueCheckStarted += () => { comboContinued = false; };
        characterModel.characterAnimEventHandler.onComboContinueCheckEnded += () =>
        {
            if (!comboContinued)
            {
                anim.SetTrigger("CancelCombo");
                this.isAttackSequenceActive = false;
            }
        };

        characterModel.characterAnimEventHandler.onMeleeDashStarted += speed =>
        {
            if (!isAttackSequenceActive)
            {
                return;
            }

            characterModel.characterMovementController.DashTowards(characterModel.lockedOnTargetPos,
                defaultDashSpeed * (speed > 0 ? speed : 1), -1);
        };
        characterModel.characterAnimEventHandler.onMeleeDashEnded += () =>
        {
            characterModel.characterMovementController.StopDashing();
        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(attackerPosition), 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        hitTimer.Update();
        shieldTimer.Update();
        parryTimer.Update();
        stunnedTimer.Update();

        if (parryTimer.expired && parryTimer.timeSinceExpiry > continuousParryThreshold)
        {
            continuousParryAttempts = 0;
        }

        if (characterModel.isDead)
        {
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsShielding", false);
            anim.SetBool("IsInHitState", false);
            return;
        }

        if (!isInHitState)
        {
            if (characterModel.characterInput.LightAttack || characterModel.characterInput.HeavyAttack)
            {
                anim.SetInteger("AttackMode", characterModel.characterInput.LightAttack ? 0 : 1);

                var selectedWoosh = $"Woosh{Random.Range(1,6)}";
                if (isAttackSequenceActive)
                {
                    if (characterModel.characterAnimEventHandler.checkingComboContinue)
                    {
                        comboContinued = true;
                        anim.SetTrigger("ContinueCombo");
                        SoundEffectsManager.Instance.Play(selectedWoosh);
                    }
                }
                else
                {
                    anim.SetTrigger("Attack");
                    SoundEffectsManager.Instance.Play(selectedWoosh);
                }
            }
        }

        if (attemptingToShield && !wasAttemptingToShield)
        {
            disableParry = !characterModel.characterInput.AttemptParry || !parryTimer.expired;
            if (!disableParry)
            {
                continuousParryAttempts++;
                parryTimer.Reset();
            }

            if (isShielding)
            {
                shieldTimer.Reset();
            }
        }

        if (currentShield)
        {
            anim.SetBool("IsShielding", isShielding);
        }
        else if (attemptingToShield && !wasAttemptingToShield)
        {
            anim.SetTrigger("ShieldAttempt");
        }

        wasAttemptingToShield = attemptingToShield;

        anim.SetBool("IsAttacking", isAttackSequenceActive);
        anim.SetBool("IsInHitState", isInHitState);

        /*
        if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetTrigger("parryKick");
        }

        */
    }

    void OnDestroy()
    {
    }

    public void Parry(CharacterModel attacker)
    {
        anim.SetTrigger("Parry");
        SoundEffectsManager.Instance.Play("ParryShieldClink");
        attacker.characterMeleeController.OnParried();
    }

    public void OnParried()
    {
        anim.SetTrigger("ParryStunned");
        stunnedTimer.Reset();
        var toTarget = characterModel.lockedOnTargetPos - transform.position;
        toTarget.y = 0;

        /*characterModel.characterMovementController.DashTowards(transform.position - toTarget, parriedEffectSpeed,
            parriedEffectDuration);*/
    }

    public void SpawnWeapon(GameObject weaponPrefab)
    {
        if (currentWeapon)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }

        if (weaponPrefab)
        {
            currentWeapon = Instantiate(weaponPrefab, weaponHolder).GetComponent<Weapon>();
        }

        OnWeaponSwitched();
    }

    public void SpawnShield(GameObject shieldPrefab)
    {
        if (currentShield)
        {
            Destroy(currentShield);
            currentShield = null;
        }

        if (shieldPrefab)
        {
            currentShield = Instantiate(shieldPrefab, shieldHolder);
        }
    }

    void OnWeaponSwitched()
    {
        anim.SetInteger("CurrentWeaponId", currentWeapon?.weaponId ?? -1);

        if (weaponCollider)
        {
            weaponCollider.enabled = false;
        }
    }

    public bool CanBlock(Vector3 attackFrom)
    {
        Vector3 toAttacker = attackFrom - transform.position;
        toAttacker.y = 0;

        float attackAngle = Vector3.Angle(new Vector3(transform.forward.x, 0.0f, transform.forward.z), toAttacker);
        return attackAngle < shieldingAngle;
    }

    [UsedImplicitly]
    void TryShield()
    {
        if (CanBlock(transform.TransformPoint(attackerPosition)))
        {
            Debug.Log("Shielded");
        }
        else
        {
            Debug.Log("Not Shielded");
        }
    }

    public void UpdateWeaponCollider(bool enable)
    {
        if (weaponCollider)
        {
            weaponCollider.enabled = enable;
        }
    }

    public bool OnIncomingAttack(CharacterModel attacker, float damage, out bool parried)
    {
        parried = false;
        if (attemptingToShield && isInParryWindow && !disableParry)
        {
            parried = true;
            Parry(attacker);
            return false;
        }

        var isHeavyAttack = attacker.characterMeleeController.isHeavyAttackSequenceActive;
        if (!isHeavyAttack)
        {
            if (isShielding && CanBlock(attacker.transform.position))
            {
                anim.SetTrigger("ShieldImpact");
                SoundEffectsManager.Instance.Play("ShieldClinking");
                return false;
            }
        }

        anim.SetInteger("HitVariation",
            attacker.characterMeleeController.isLightAttackSequenceActive ? Random.Range(0, 2) : 1);
        characterModel.health.TakeDamage(damage);

        hitTimer.Reset();

        return true;
    }
}