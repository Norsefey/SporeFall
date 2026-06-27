using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    EnemyController enemyController;

    [Header("Animations")]
    [SerializeField] protected Animator animator;
    protected bool isRising = false;
    [SerializeField] protected float risingAnimationLength = 2;
    public Animator Animator => animator;

    public bool spawnInGround = true;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();

        enemyController.OnStateChange += UpdateAnimation;
    }

    private void Start()
    {
        if(spawnInGround)
            TriggerRiseAnimation();
    }

    private void UpdateAnimation(EnemyState state)
    {
        switch(state)
        {
            case EnemyState.Idle:
                animator.SetInteger("State", 0);
                break;
            case EnemyState.Searching:
                animator.SetInteger("State", 0);
                break;
            case EnemyState.Moving:
                animator.SetInteger("State", 1);
                break;
            case EnemyState.AtTarget:
                animator.SetInteger("State", 0);
                break;
            case EnemyState.Attacking:
                animator.SetInteger("State", 0);
                break;

            default:
                break;

        }
    }
    public void TriggerRiseAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Rise");
            StartCoroutine(RisingFromGround());
        }
    }
    protected IEnumerator RisingFromGround()
    {
        enemyController.TransitionTo(EnemyState.Idle);
        
        isRising = true;
        yield return new WaitForSeconds(risingAnimationLength);

        isRising = false;
        enemyController.TransitionTo(EnemyState.Searching);
    }
}
