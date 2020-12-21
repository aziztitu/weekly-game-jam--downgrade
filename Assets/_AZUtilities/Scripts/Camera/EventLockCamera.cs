using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class EventLockCamera : MonoBehaviour
{
    public class StateData
    {
        public Transform LookAtTarget = null;
        public Transform FollowTarget = null;
//        public bool jumpCut = false;
        public float EventLockDuration;
    }

    public CameraFollow cameraFollow;

    private CinemachineVirtualCamera _virtualCamera;
    private StatefulCinemachineCamera _statefulCinemachineCamera;

    private void Awake()
    {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _statefulCinemachineCamera = GetComponent<StatefulCinemachineCamera>();

        cameraFollow.Init(_virtualCamera);
        cameraFollow.transform.position = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        _statefulCinemachineCamera.OnActivated.AddListener((statefulCamera) =>
        {
            StateData stateData = (StateData) statefulCamera.stateData;
            cameraFollow.transform.position = CinemachineCameraManager.Instance.brain.transform.position;
            UpdateEventLock(stateData);
        });

        _statefulCinemachineCamera.OnDeactivated.AddListener((statefulCamera) =>
        {
//            _virtualCamera.LookAt = null;
//            _virtualCamera.Follow = null;
        });

        _statefulCinemachineCamera.OnStateUpdated.AddListener(statefulCamera =>
        {
            StateData stateData = (StateData)statefulCamera.stateData;
            UpdateEventLock(stateData);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.End) && CinemachineCameraManager.Instance.CurrentState ==
            CinemachineCameraManager.CinemachineCameraState.EventLock)
        {
            CinemachineCameraManager.Instance.SwitchToPreviousCameraState();
        }
    }

    void UpdateEventLock(StateData stateData)
    {
        if (stateData != null)
        {
            if (stateData.FollowTarget)
            {
                cameraFollow.SetFollow(stateData.FollowTarget);
                cameraFollow.SetLookAt(stateData.LookAtTarget);

                //                    _virtualCamera.LookAt = stateData.LookAtTarget;
                //                    _virtualCamera.Follow = stateData.FollowTarget;

                if (stateData.EventLockDuration > 0)
                {
                    StartCoroutine(WaitAndEndEventLock(stateData.EventLockDuration));
                }
            }
        }
    }

    IEnumerator WaitAndEndEventLock(float totalDuration)
    {
        yield return new WaitForSecondsRealtime(totalDuration);
        CinemachineCameraManager.Instance.SwitchToPreviousCameraState();
    }
}