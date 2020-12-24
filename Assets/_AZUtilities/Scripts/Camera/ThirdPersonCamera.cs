using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ThirdPersonCamera : SingletonMonoBehaviour<ThirdPersonCamera>
{
    public bool isSwitching => cameraFollow.targetFollow != null;

    public Transform targetObject;

    public CinemachineVirtualCamera virtualCamera { get; private set; }
    public CameraFollow cameraFollow;

    public float minXSensitivity = 50;
    public float maxXSensitivity = 100;

    public float minYSensitivity = 50;
    public float maxYSensitivity = 180;

    private StatefulCinemachineCamera _statefulCinemachineCamera;
    private CinemachinePOV _cinemachinePov;

    new void Awake()
    {
        base.Awake();

        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        _statefulCinemachineCamera = GetComponentInChildren<StatefulCinemachineCamera>();
        _cinemachinePov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();

        cameraFollow.Init(virtualCamera);
    }

    // Use this for initialization
    void Start()
    {
        HelperUtilities.UpdateCursorLock(true);

//        RefreshFromSettings();
//        GameManager.Instance.onGlobalSettingsChanged += RefreshFromSettings;

        _statefulCinemachineCamera.OnActivated.AddListener(statefulCineCam =>
        {
            if (targetObject)
            {
                cameraFollow.SetFollow(targetObject);
                _statefulCinemachineCamera.CamNoise(0, 0);
            }
        });
    }

    void Update()
    {
    }

    void OnDestroy()
    {
        /*if (GameManager.Instance)
        {
//            GameManager.Instance.onGlobalSettingsChanged -= RefreshFromSettings;
        }*/
    }

    public void SetTargetObject(Transform target)
    {
        targetObject = target;
        cameraFollow.SetFollow(targetObject);
    }

    /*void RefreshFromSettings()
    {
        var gameManager = GameManager.Get();

        var maxXSpeed = HelperUtilities.Remap(gameManager.globalSettingsData.mouseXSensitivity, 0, 1, minXSensitivity,
            maxXSensitivity);
        var maxYSpeed = HelperUtilities.Remap(gameManager.globalSettingsData.mouseYSensitivity, 0, 1, minYSensitivity,
            maxYSensitivity);

        _cinemachinePov.m_HorizontalAxis.m_MaxSpeed = maxXSpeed;
        _cinemachinePov.m_VerticalAxis.m_MaxSpeed = maxYSpeed;
        _cinemachinePov.m_VerticalAxis.m_InvertInput = !gameManager.globalSettingsData.invertMouseY;
    }*/
}