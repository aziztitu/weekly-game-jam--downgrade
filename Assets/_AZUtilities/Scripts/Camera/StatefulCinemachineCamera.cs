using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class StatefulCinemachineCamera : MonoBehaviour
{
    [Serializable]
    public class StatefulCinemachineCameraEvent : UnityEvent<StatefulCinemachineCamera>
    {
    }

    public CinemachineCameraManager.CinemachineCameraState cinemachineCameraState;
    public StatefulCinemachineCameraEvent OnActivated;
    public StatefulCinemachineCameraEvent OnDeactivated;
    public StatefulCinemachineCameraEvent OnStateUpdated;

    public CinemachineVirtualCamera virtualCamera { get; private set; }
    public object stateData { get; private set; }

    public bool IsActive { get; private set; }

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void Activate(object stateData = null)
    {
        IsActive = true;
        this.stateData = stateData;
        virtualCamera.enabled = true;
        OnActivated.Invoke(this);
    }

    public void Deactivate()
    {
        IsActive = false;
        stateData = null;
        virtualCamera.enabled = false;
        OnDeactivated.Invoke(this);
    }

    public void UpdateStateData(object stateData = null)
    {
        this.stateData = stateData;
        OnStateUpdated.Invoke(this);
    }

    public void CamNoise(float amplitudeGain, float frequencyGain)
    {
        var noiseParams = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        noiseParams.m_AmplitudeGain = amplitudeGain;
        noiseParams.m_FrequencyGain = frequencyGain;
    }
}