using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CharacterModel : MonoBehaviour
{
    public class CharacterInput
    {
        public Vector3 Move;
        //        public bool Sprint;
    }

    public CharacterInput characterInput = new CharacterInput();
    
    public CharacterMovementController characterMovementController { get; private set; }
    public Animator animator { get; private set; }
    public Health health { get; private set; }
    public bool isAlive => health.currentHealth > 0;
    public bool isDead => !isAlive;

    public Vector3 lockedOnTargetPos => Vector3.zero;

    // public float delayBeforeHealthRegeneration = 3f;
    // public float healthRegenerationSpeed = 1f;
    public Transform playerTarget;
    public float deathAnimationDuration = 3f;
    public float playerHitCamShakeDuration = 1f;
    public float playerHitCamShakeMinInterval = 1f;
    public float playerHitCamShakeAmplitude = 3f;
    public int playerHitCamShakeFrequency = 10;

    private bool isShakingCam = false;

    void Awake()
    {
        characterMovementController = GetComponent<CharacterMovementController>();
        animator = GetComponentInChildren<Animator>(false);
        health = GetComponent<Health>();
        health.OnDamageTaken.AddListener(() =>
        {
            if (health.currentHealth > 0)
            {
                animator.SetTrigger("Hit");

                if (!isShakingCam)
                {
                    CinemachineCameraManager.Instance.CurrentStatefulCinemachineCamera.CamNoise(
                        playerHitCamShakeAmplitude, playerHitCamShakeFrequency);
                    isShakingCam = true;
                    this.WaitAndExecute(() =>
                    {
                        CinemachineCameraManager.Instance.CurrentStatefulCinemachineCamera.CamNoise(0, 0);
                        this.WaitAndExecute(() =>
                        {
                            isShakingCam = false;
                        }, playerHitCamShakeMinInterval);
                    }, playerHitCamShakeDuration);
                }
            }
        });
        health.OnHealthDepleted.AddListener(() =>
        {
            animator.applyRootMotion = true;
            animator.SetTrigger("Death");
        });
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        /*if (isAlive && health.currentHealth < health.maxHealth &&
            health.timeSinceLastDamage > delayBeforeHealthRegeneration)
        {
            health.UpdateHealth(healthRegenerationSpeed * Time.deltaTime);
        }*/
    }
}