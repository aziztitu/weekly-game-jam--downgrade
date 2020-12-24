using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var charModel = animator.GetComponentInParent<CharacterModel>();

        if (charModel.characterMeleeController.isAttackSequenceActive)
        {
            charModel.characterAnimEventHandler.MeleeAttackSequenceEnd();
        }

        charModel.characterMeleeController.isInHitState = true;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var charModel = animator.GetComponentInParent<CharacterModel>();
        charModel.characterMeleeController.isInHitState = false;
    }
}
