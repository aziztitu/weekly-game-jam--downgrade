using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MeleeAttackSequenceState : StateMachineBehaviour
{
    public bool isEntry = false;
    public bool isExit = false;

    public bool applyRootMotion = false;
    public bool stopDashingOnExit = true;

    [Header("Avatar Position Offset")] public Vector3 avatarPositionOffset;
    public float positionTweenDuration;
    public bool translateAvatarOnEnter = false;
    public bool translateAvatarOnExit = false;

    [Header("Avatar Rotation Offset")] public Vector3 avatarRotationOffset;
    public float rotationTweenDuration;
    public bool rotateAvatarOnEnter = false;
    public bool rotateAvatarOnExit = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var charModel = animator.GetComponentInParent<CharacterModel>();
        if (isEntry)
        {
            charModel.characterAnimEventHandler.MeleeAttackSequenceStart();
        }
        
        if (translateAvatarOnEnter)
        {
            charModel.avatarModel
                .DOLocalMove(avatarPositionOffset, positionTweenDuration).Play();
        }

        if (rotateAvatarOnEnter)
        {
            charModel.avatarModel
                .DOLocalRotateQuaternion(Quaternion.Euler(avatarRotationOffset), rotationTweenDuration).Play();
        }

        if (applyRootMotion)
        {
            charModel.avatarModel.localPosition = Vector3.zero;
            animator.applyRootMotion = true;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var charModel = animator.GetComponentInParent<CharacterModel>();

        if (applyRootMotion)
        {
            animator.applyRootMotion = false;
        }
        
        if (translateAvatarOnExit || applyRootMotion)
        {
            charModel.avatarModel
                .DOLocalMove(Vector3.zero, positionTweenDuration).Play();
        }

        if (rotateAvatarOnExit || applyRootMotion)
        {
            charModel.avatarModel.DOLocalRotateQuaternion(Quaternion.identity, rotationTweenDuration).Play();
        }

        if (isExit)
        {
            charModel.characterAnimEventHandler.MeleeAttackSequenceEnd(stopDashingOnExit);
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}