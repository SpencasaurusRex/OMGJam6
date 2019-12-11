using UnityEngine;

public class Laser : MonoBehaviour
{
    public RadialPosition TargetPosition;
    public float Speed;

    public delegate void LaserReachedDestination(RadialPosition pos);
    public event LaserReachedDestination OnLaserReachedDestination;
    Animator animator;

    bool hit;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!hit)
        {
            var target = BoardController.Instance.GetPosition(TargetPosition, false) * 10;
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, target, Speed * Time.deltaTime);
        }
        else
        {
            var currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (hit && currentState.normalizedTime > 1 && currentState.IsName("LaserHit"))
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.TryGetComponent<Orb>(out var orb))
        {
            print("Hit " + collider.gameObject.name);
            OnLaserReachedDestination?.Invoke(TargetPosition);
            animator.SetBool("Hit", true);
            hit = true;
        }
    }
}
