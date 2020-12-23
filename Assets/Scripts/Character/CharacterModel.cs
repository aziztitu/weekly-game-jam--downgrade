﻿using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterModel : MonoBehaviour
{
    public class CharacterInput
    {
        public Vector3 Move;
        public bool LightAttack;
        public bool HeavyAttack;
        public bool IsBlocking;
        public bool AttemptParry;

        public bool Dodge;
        public bool Taunt;
        //        public bool Sprint;
    }

    public CharacterInput characterInput = new CharacterInput();

    public CharacterMovementController characterMovementController { get; private set; }
    public PlayerInputController playerInputController { get; private set; }
    public AIController aiController { get; private set; }
    public MeleeTest characterMeleeController { get; private set; }
    public CharacterAnimEventHandler characterAnimEventHandler { get; private set; }
    public Animator animator { get; private set; }
    public Health health { get; private set; }
    public bool isAlive => health.currentHealth > 0;
    public bool isDead => !isAlive;

    [HideInInspector] public BattleManager.CharacterSelection characterSelectionData = null;
    [ReadOnly] public int characterIndex = 0;

    public bool isLocalPlayer => characterSelectionData?.isLocalPlayer ?? false;

    [HideInInspector] public Transform lockedOnTarget;
    public Vector3 lockedOnTargetPos => lockedOnTarget?.position ?? Vector3.zero;

    // public float delayBeforeHealthRegeneration = 3f;
    // public float healthRegenerationSpeed = 1f;
    public Transform playerTarget;
    public Transform avatar;
    public Transform avatarModel
    {
        get
        {
            for (int i = 0; i < avatar.childCount; i++)
            {
                var child = avatar.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    return child;
                }
            }

            return null;
        }
    }

    public float deathAnimationDuration = 3f;
    public float playerHitCamShakeDuration = 1f;
    public float playerHitCamShakeMinInterval = 1f;
    public float playerHitCamShakeAmplitude = 3f;
    public int playerHitCamShakeFrequency = 10;

    private bool isShakingCam = false;

    public event Action onInitialized;

    void Awake()
    {
        characterMovementController = GetComponent<CharacterMovementController>();
        playerInputController = GetComponent<PlayerInputController>();
        aiController = GetComponent<AIController>();
        characterMeleeController = GetComponent<MeleeTest>();
        animator = GetComponentInChildren<Animator>(false);
        characterAnimEventHandler = animator.GetComponent<CharacterAnimEventHandler>();
        health = GetComponent<Health>();
        health.OnDamageTaken.AddListener(() =>
        {
            if (health.currentHealth > 0)
            {
                animator.SetTrigger("Hit");

                if (isLocalPlayer && !isShakingCam)
                {
                    CinemachineCameraManager.Instance.CurrentStatefulCinemachineCamera.CamNoise(
                        playerHitCamShakeAmplitude, playerHitCamShakeFrequency);
                    isShakingCam = true;
                    this.WaitAndExecute(() =>
                    {
                        CinemachineCameraManager.Instance.CurrentStatefulCinemachineCamera.CamNoise(0, 0);
                        this.WaitAndExecute(() => { isShakingCam = false; }, playerHitCamShakeMinInterval);
                    }, playerHitCamShakeDuration);
                }
            }
        });
        health.OnHealthDepleted.AddListener(() =>
        {
            animator.applyRootMotion = true;
            var deathMode = (Random.Range(0, 10) >= 5) ? 1 : 2;
            animator.SetTrigger($"Death{deathMode}");
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

    public void OnInitialized()
    {
        this.onInitialized?.Invoke();
    }
}