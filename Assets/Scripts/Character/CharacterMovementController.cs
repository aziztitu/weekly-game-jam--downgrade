using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterMovementController : MonoBehaviour
{
    public float acceleration = 2f;
    public float deceleration = 2f;
    public float turnSpeed = 3f;

    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] private float m_HitSpeedFactor;
    [SerializeField] private float m_HitSpeedDuration;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private float m_StepInterval;

    [SerializeField]
    private AudioClip[] m_FootstepSounds; // an array of footstep sounds that will be randomly selected from.

    [SerializeField] private AudioClip m_JumpSound; // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound; // the sound played when character touches back on ground.

    private CharacterModel characterModel;
    private bool m_Jump;
    private float m_YRotation;
    private float curSpeed;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    public CharacterController m_CharacterController { get; private set; }
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;
    private float lastJumpTime = float.MinValue;
    [SerializeField] [ReadOnly] private float chargedJumpCharge = 0;
    private float airTime = 0;

    private class GizmosData
    {
        public Vector3 raycastDir;
    }

    private GizmosData _gizmosData = new GizmosData();

    private void Awake()
    {
        characterModel = GetComponent<CharacterModel>();
    }

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 10);

        Gizmos.color = Color.red;
        if (characterModel)
        {
            Gizmos.DrawRay(transform.position, ThirdPersonCamera.Instance.virtualCamera.transform.TransformDirection(
                                                   characterModel.characterInput.Move
                                                       .normalized).normalized * 10);
        }

        // Gizmos.DrawRay(transform.position, _gizmosData.raycastDir * 10);
        // Gizmos.DrawWireSphere(transform.position, 10f);
    }


    // Update is called once per frame
    private void Update()
    {
        if (characterModel.isDead)
        {
            return;
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    private void FixedUpdate()
    {
        if (characterModel.isDead)
        {
            UpdateMoveAnimation(0, 0, true, false);
            return;
        }

        float speed;
        GetInput(out speed);

        Move(speed);

        if (m_CharacterController.isGrounded)
        {
            airTime = 0;
        }
        else
        {
            airTime += Time.fixedDeltaTime;
        }

        ProgressStepCycle(speed);
    }

    private void Move(float speed)
    {
        if (speed > curSpeed)
        {
            curSpeed += acceleration;
            curSpeed = Mathf.Min(curSpeed, speed);
        }
        else if (speed < curSpeed)
        {
            curSpeed -= deceleration;
            curSpeed = Mathf.Max(curSpeed, speed);
        }

        switch (CinemachineCameraManager.Instance.CurrentState)
        {
            case CinemachineCameraManager.CinemachineCameraState.ThirdPerson:
                TPMove(curSpeed);
                break;
            default:
                return;
        }

        if (!m_Jumping && m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;
        }
        else
        {
            //            Debug.Log("Applying Gravity");
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }

        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        // Move Animation

        /*if (m_IsWalking)
        {
            animVector /= 2;
        }*/

        //Vector3 toLockTarget = (characterModel.lockedOnTargetPos - transform.position).normalized;
        float angleToLockTarget = Vector3.SignedAngle(transform.forward,
                                      ThirdPersonCamera.Instance.virtualCamera.transform.TransformDirection(
                                          characterModel.characterInput.Move
                                              .normalized), Vector3.up) * Mathf.Deg2Rad;

        float curSpeedFactor = HelperUtilities.Remap(curSpeed, 0, m_IsWalking ? m_WalkSpeed : m_RunSpeed, 0, 1) *
                               characterModel.characterInput.Move.magnitude;

        Vector3 animVector = new Vector3(Mathf.Sin(angleToLockTarget), Mathf.Cos(angleToLockTarget)).normalized *
                             curSpeedFactor;
        if (animVector.magnitude > 1)
        {
            animVector.Normalize();
        }

        UpdateMoveAnimation(animVector.y, animVector.x,
            m_CharacterController.isGrounded, !m_IsWalking);
    }

    void UpdateMoveAnimation(float forward, float right, bool isGrounded, bool isSprinting)
    {
        characterModel.animator.SetFloat("Forward", forward);
        characterModel.animator.SetFloat("Right", right);

        characterModel.animator.SetBool("IsGrounded", isGrounded);
        characterModel.animator.SetBool("IsSprinting", isSprinting);
    }

    private void TPMove(float speed)
    {
        ThirdPersonCamera thirdPersonCamera = ThirdPersonCamera.Instance;

        Vector3 desiredMove = Vector3.zero;
        if (thirdPersonCamera)
        {
            Vector3 forwardDir = thirdPersonCamera.virtualCamera.transform.forward;
            Vector3 rightDir = thirdPersonCamera.virtualCamera.transform.right;

            forwardDir.y = 0;
            rightDir.y = 0;

            forwardDir.Normalize();
            rightDir.Normalize();

            desiredMove = forwardDir * m_Input.y;
            desiredMove += rightDir * m_Input.x;
        }

        if (desiredMove.magnitude > 1)
        {
            desiredMove.Normalize();
        }

        if (desiredMove.magnitude > 0)
        {
            //            Vector3 lookAtTarget = transform.position +
            //                                   (thirdPersonCamera.virtualCamera.transform.forward * 5);
            // Vector3 lookAtTarget = transform.position + (desiredMove * 5);
            Vector3 lookAtTarget = characterModel.lockedOnTargetPos;
            lookAtTarget.y = transform.position.y;

            Vector3 targetForward = lookAtTarget - transform.position;
            targetForward.Normalize();

//            transform.forward = Vector3.Lerp(transform.forward, targetForward, turnSpeed * Time.fixedDeltaTime);

            var targetRot = Quaternion.LookRotation(targetForward);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;
    }

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle +=
                (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded || m_FootstepSounds.Length == 0)
        {
            return;
        }

        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    private void GetInput(out float speed)
    {
        CharacterModel.CharacterInput playerInput = characterModel.characterInput;

        if (characterModel.characterMeleeController.isAttackSequenceActive)
        {
            //playerInput.Move = new Vector3(0, 0, 0);
            //curSpeed = 0;
            speed = 0;
            return;
        }

        // Read input

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run chaseSpeed is modified by a key press.
        // keep track of whether or not the character is walking or running
        // m_IsWalking = characterModel.characterInput.Sprint;
        m_IsWalking = true;
#endif
        // set the desired chaseSpeed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;

        if (characterModel.health.timeSinceLastDamage < m_HitSpeedDuration)
        {
            speed *= m_HitSpeedFactor;
        }

        m_Input = new Vector2(playerInput.Move.x, playerInput.Move.z);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }

        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}