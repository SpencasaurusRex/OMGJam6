using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Orb : MonoBehaviour
{
    // Configuration
    public bool Shoot;
    public bool BounceBack;
    public float BreakDelayAdd;
    public PitchVariance Pitch;
    public AudioClip ShatterSound;

    public Vector2 StraightBump;
    public Vector2 DiagonalBump;

    public GameObject CapPrefab;
    public GameObject SidePrefab;

    // Runtime
    public int Type;
    AudioSource audioSource;
    public bool Shattering;
    public float ShatterCountdown;
    BoardMover mover;
    float pitchMultiplier;
    bool ShatteringAnimation;
    Animator animator;
    GameObject chainIndicator;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Pitch = GetComponent<PitchVariance>();
        mover = GetComponent<BoardMover>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (ShatteringAnimation)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            {
                Destroy(gameObject);
            }
        }
        else if (Shattering)
        {
            ShatterCountdown -= Time.deltaTime;
            if (ShatterCountdown <= 0)
            {
                BoardController.Instance.RemoveMover(mover);
                ShatteringAnimation = true;
                animator.enabled = true;
                Factory.Instance.PlaySound(ShatterSound, pitchMultiplier);
            }
        }
    }

    public void CloseEnough()
    {
        if (Shoot)
        {
            Shoot = false;
            BounceBack = true;
            if (mover.Position.Position < BoardController.NUM_SPACES - 1)
            {
                audioSource.pitch = Pitch.GetRandomPitch();
                audioSource.Play();
            }
            mover.Offset = Vector2.zero;
            mover.LinearSpeed /= 2;
        }
        else if (BounceBack)
        {
            BounceBack = false;
            mover.MovementType = MovementType.Lerp;
            CheckForChain();
        }
    }

    public void Shatter(int delayMultiplier)
    {
        Shattering = true;
        ShatterCountdown = delayMultiplier * BreakDelayAdd;

        pitchMultiplier = Mathf.Pow(9f / 8, delayMultiplier);
        mover.Locked = true;

        Destroy(chainIndicator);
    }

    public void CheckForChain()
    {
        List<Orb> chainOrbs = new List<Orb> {this};

        var pos = mover.Position;
        for (int i = pos.Position + 1; i < BoardController.NUM_SPACES - 1; i++)
        {
            var mover = BoardController.Instance.GetMover(new RadialPosition(pos.Lane, i));
            if (mover.TryGetComponent<Orb>(out var orb) && orb.Type == Type)
            {
                chainOrbs.Add(orb);
            }
            else
            {
                break;
            }
        }

        if (chainOrbs.Count >= 3)
        {
            for (int i = 0; i < chainOrbs.Count; i++)
            {
                chainOrbs[i].AddChain(i == 0,i == chainOrbs.Count - 1);
            }
        }
    }

    public void AddChain(bool cap, bool endCap)
    {
        if (chainIndicator != null)
        {
            Destroy(chainIndicator);
        }

        Quaternion rotation = Quaternion.Euler(0, 0, mover.Position.Lane / 8f * 360f + (!endCap ? 180 : 0) - 90);
        if (cap || endCap)
        {
            chainIndicator = Instantiate(CapPrefab, transform);
        }
        else
        {
            chainIndicator = Instantiate(SidePrefab, transform);
        }
        chainIndicator.transform.rotation = rotation;
    }
}
