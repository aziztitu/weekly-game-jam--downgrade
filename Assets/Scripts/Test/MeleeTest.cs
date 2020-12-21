using System.Collections;
using System.Collections.Generic;
using BasicTools.ButtonInspector;
using JetBrains.Annotations;
using UnityEngine;

public class MeleeTest : MonoBehaviour
{
    Animator anim => characterModel.animator;

    private CharacterModel characterModel;
    public GameObject currentWeapon;
    Collider weaponCollider;

    public GameObject currentShield;
    public float shieldingAngle = 0;
    public float parryWindow = 0.5f;

    [Header("Debug")] public Vector3 attackerPosition;

    [Button("Try Shield", "TryShield")] [SerializeField]
    private bool _btnTryShield;

    public bool isShielding => !isAttackSequenceActive && characterModel.characterInput.IsBlocking;
    private bool wasShielding = false;
    private float lastShieldStartTime = 0;
    public float timeSinceLastShieldStartTime => Time.time - lastShieldStartTime;
    public bool isInParryWindow => timeSinceLastShieldStartTime < parryWindow;

    public bool isAttackSequenceActive { get; private set; } = false;

    private void Awake()
    {
        characterModel = GetComponent<CharacterModel>();
    }

    // Start is called before the first frame update
    void Start()
    {
        OnWeaponSwitched();

        characterModel.characterAnimEventHandler.onMeleeAttackSequenceStarted +=
            () => { this.isAttackSequenceActive = true; };
        characterModel.characterAnimEventHandler.onMeleeAttackSequenceEnded +=
            () => { this.isAttackSequenceActive = false; };

        characterModel.characterAnimEventHandler.onMeleeAttackStarted += (int comboIndex, float damage) =>
        {
            UpdateWeaponCollider(true);
        };
        characterModel.characterAnimEventHandler.onMeleeAttackEnded += () => { UpdateWeaponCollider(false); };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(attackerPosition), 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (characterModel.characterInput.LightAttack)
        {
            if (isAttackSequenceActive)
            {
                if (characterModel.characterAnimEventHandler.checkingComboContinue)
                {
                    anim.SetTrigger("ContinueCombo");
                }
            }
            else
            {
                anim.SetTrigger("Attack");
            }
        }

        if (isShielding && !wasShielding)
        {
            lastShieldStartTime = Time.time;
        }

        anim.SetBool("IsShielding", isShielding);
        wasShielding = isShielding;

        /*if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetTrigger("left");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("right");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetTrigger("parryKick");
        }

        */
    }

    void OnDestroy()
    {
    }

    public void Parry()
    {
        anim.SetTrigger("parry");
    }

    void OnWeaponSwitched()
    {
        if (currentWeapon)
        {
            weaponCollider = currentWeapon.GetComponent<Collider>();
            weaponCollider.enabled = false;
        }
        else
        {
            weaponCollider = null;
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
}