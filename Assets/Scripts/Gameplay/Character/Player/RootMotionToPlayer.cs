using UnityEngine;

public class RootMotionToPlayer : MonoBehaviour
{
    IRootMotionParent rootMotionParent;
    Animator animator;

    void Awake()
    {
        rootMotionParent = transform.parent.GetComponent<IRootMotionParent>();
        animator = GetComponent<Animator>();
    }

    void OnAnimatorMove()
    {
        Vector3 delta = animator.deltaPosition;
        rootMotionParent.UpdateRootMotionDelta(delta);
    }
}
