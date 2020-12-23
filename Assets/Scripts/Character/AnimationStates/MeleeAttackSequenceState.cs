using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackSequenceState : StateMachineBehaviour
{
    public bool isEntry = false;
    public bool isExit = false;

    public float avatarYRotationOffset = 0;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isEntry)
        {
            var charModel = animator.GetComponentInParent<CharacterModel>();
            charModel.characterAnimEventHandler.MeleeAttackSequenceStart();
        }

//        animator.applyRootMotion = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isExit)
        {
            var charModel = animator.GetComponentInParent<CharacterModel>();
            if (charModel.characterMeleeController.isAttackSequenceActive)
            {
                charModel.characterAnimEventHandler.MeleeAttackEnd();
            }
            charModel.characterAnimEventHandler.MeleeAttackSequenceEnd();
            animator.applyRootMotion = false;
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
