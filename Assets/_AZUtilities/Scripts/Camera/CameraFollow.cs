using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float switchMoveSpeed = 5f;
    public float switchRotateSpeed = 5f;
    public float switchRotateStartTolerance = 5f;
    public float switchDistanceTolerance = 1f;
    public float switchAngleTolerance = 1f;

    public bool moveInsideFollowTarget = true;
    public bool disableAimWhileSwitching = true;

    public Transform targetFollow { get; private set; }
    public Transform lookAtTarget { get; private set; }

    public event Action OnSwitchingStarted;
    public event Action OnSwitchingEnded;

    [SerializeField] [ReadOnly] private CinemachineVirtualCamera _virtualCamera = null;
    private CinemachinePOV pov;
    private bool _rotateDuringSwitch = true;

    // Start is called before the first frame update
    void Start()
    {
        pov = _virtualCamera.GetCinemachineComponent<CinemachinePOV>();

        OnSwitchingStarted += () =>
        {
            if (disableAimWhileSwitching && pov)
            {
                pov.enabled = false;
            }
        };

        OnSwitchingEnded += () =>
        {
            if (pov)
            {
                pov.ResetRotation(_virtualCamera.transform.rotation);
                if (disableAimWhileSwitching)
                {
                    pov.enabled = true;
                }
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Init(CinemachineVirtualCamera virtualCamera, bool rotateDuringSwitch = true)
    {
        _virtualCamera = virtualCamera;
        _virtualCamera.Follow = transform;
        _rotateDuringSwitch = rotateDuringSwitch;
    }

    public void UpdateSpeed(float speed)
    {
        switchMoveSpeed = speed;
    }

    private void FixedUpdate()
    {
        if (_virtualCamera && targetFollow)
        {
//            var charMovementController = GetComponentInParent<PlayerMovementController>();
            var targetRot = targetFollow.rotation;
            if (lookAtTarget)
            {
                targetRot = Quaternion.LookRotation(lookAtTarget.position - targetFollow.position);
            }

            /*if (charMovementController)
            {
                var mouseLook = charMovementController.MouseLook;
                targetRot = mouseLook.CameraTargetRot;
                var targetAngles = targetRot.eulerAngles;
                targetAngles.y = mouseLook.CharacterTargetRot.eulerAngles.y;

                targetRot = Quaternion.Euler(targetAngles);
            }*/


            transform.position = Vector3.Lerp(transform.position, targetFollow.position,
                switchMoveSpeed * Time.fixedDeltaTime);


            float distToTarget = Vector3.Distance(transform.position, targetFollow.position);

            bool closePos = distToTarget <= switchDistanceTolerance;


            if (_rotateDuringSwitch && (switchRotateStartTolerance < 0 || distToTarget <= switchRotateStartTolerance))
            {
//                print(targetRot.eulerAngles);
                _virtualCamera.transform.rotation = Quaternion.Slerp(_virtualCamera.transform.rotation,
                    targetRot,
                    switchRotateSpeed * Time.fixedDeltaTime);
            }

            bool closeAngle = Quaternion.Angle(_virtualCamera.transform.rotation, targetRot) <=
                              switchAngleTolerance;

            if (closePos && (closeAngle || !_rotateDuringSwitch))
            {
                if (moveInsideFollowTarget)
                {
                    transform.SetParent(targetFollow);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    targetFollow = null;

                    OnSwitchingEnded?.Invoke();
                }
            }
        }
    }

    public void SetFollow(Transform targetFollow)
    {
        this.targetFollow = targetFollow;

        if (this.targetFollow == null || CinemachineCameraManager.Instance.brain.IsBlending)
        {
            if (moveInsideFollowTarget)
            {
                transform.SetParent(this.targetFollow);
            }
        }
        else
        {
            OnSwitchingStarted?.Invoke();
        }
    }

    public void SetLookAt(Transform lookAt)
    {
        lookAtTarget = lookAt;
    }
}