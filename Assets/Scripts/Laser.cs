using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public RadialPosition TargetPosition;
    public float Speed;
    public Player player;

    public delegate void LaserHit(Orb orb);
    public event LaserHit OnLaserHit;
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
            var target = BoardController.Instance.GetPosition(TargetPosition) * 100;
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, target, Speed * Time.deltaTime);
            if (((Vector2)transform.localPosition - target).magnitude < .01f)
            {
                Destroy(gameObject);
            }
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
        if (!hit && collider.gameObject.TryGetComponent<Orb>(out var orb))
        {
            if (orb.Shattering) return;

            OnLaserHit?.Invoke(orb);
            animator.SetBool("Hit", true);
            hit = true;

            var orbPosition = orb.GetComponent<BoardMover>().Position;
            var lane = BoardController.Instance.GetLane(orbPosition.Lane);
            var orbsToBreak = new List<Orb>();
            orbsToBreak.Add(orb);

            for (int i = orbPosition.Position + 1; i < BoardController.NUM_SPACES; i++)
            {
                if (lane[i] == null) break;
                var laneOrb = lane[i].GetComponent<Orb>();
                if (laneOrb.Type == orb.Type)
                {
                    orbsToBreak.Add(laneOrb);
                }
                else break;
            }

            for (int i = 0; i < orbsToBreak.Count; i++)
            {
                orbsToBreak[i].Shatter(i);
            }

            player.ChargeGun(orbsToBreak.Count);
        }
    }
}
