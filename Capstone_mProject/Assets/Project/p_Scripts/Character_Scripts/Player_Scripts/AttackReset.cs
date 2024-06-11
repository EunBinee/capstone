using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackReset : StateMachineBehaviour
{
    [SerializeField] string triggerName;

    private void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.ResetTrigger(triggerName);
    }
}
