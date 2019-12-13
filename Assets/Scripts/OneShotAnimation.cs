using UnityEngine;

public class OneShotAnimation : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.normalizedTime > 1)
        {
            Destroy(gameObject);
        }
    }
}
