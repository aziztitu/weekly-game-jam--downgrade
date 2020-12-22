using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimEventHandler : MonoBehaviour
{
    public event Action onDeathEnded;
    public event Action onShieldOpenRequested;
    public event Action onMeleeAttackSequenceStarted;
    public event Action onMeleeAttackSequenceEnded;
    public event Action<int, float> onMeleeAttackStarted;
    public event Action onMeleeAttackEnded;
    public event Action onComboContinueCheckStarted;
    public event Action onComboContinueCheckEnded;
    public event Action<float> onZoomRequestStarted;
    public event Action onZoomRequestEnded;

    private Animator animator;
    private CharacterModel owner;

    public bool checkingComboContinue = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        owner = GetComponentInParent<CharacterModel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeathEnd()
    {
        onDeathEnded?.Invoke();
    }

    public void RequestShieldOpen()
    {
        onShieldOpenRequested?.Invoke();
    }

    public void MeleeAttackSequenceStart()
    {
        Debug.Log("Melee Attack Sequence Started");
        onMeleeAttackSequenceStarted?.Invoke();
    }

    public void MeleeAttackSequenceEnd()
    {
        Debug.Log("Melee Attack Sequence Ended");
        checkingComboContinue = false;
        onMeleeAttackSequenceEnded?.Invoke();

        animator.ResetTrigger("Attack");
    }

    public void MeleeAttackStart(AnimationEvent animationEvent)
    {
        Debug.Log($"Melee Attack Started: {animationEvent.intParameter}, {animationEvent.floatParameter}");
        onMeleeAttackStarted?.Invoke(animationEvent.intParameter, animationEvent.floatParameter);
    }

    public void MeleeAttackEnd()
    {
        Debug.Log("Melee Attack Ended");
        onMeleeAttackEnded?.Invoke();
    }

    public void ComboContinueCheckStart()
    {
        checkingComboContinue = true;
        animator.ResetTrigger("ContinueCombo");
        onComboContinueCheckStarted?.Invoke();
    }

    public void ComboContinueCheckEnd()
    {
        checkingComboContinue = false;
        onComboContinueCheckEnded?.Invoke();
    }

    public void ZoomStart(AnimationEvent animationEvent)
    {
        onZoomRequestStarted?.Invoke(animationEvent.floatParameter);
    }

    public void ZoomEnd()
    {
        onZoomRequestEnded?.Invoke();
    }
}
