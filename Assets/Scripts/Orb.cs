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

    // Runtime
    public int Type;
    AudioSource audioSource;
    public bool Shattering;
    public float ShatterCountdown;
    BoardMover mover;
    float pitchMultiplier;
    bool ShatteringAnimation;
    Animator animator;

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
        }
    }

    public void Shatter(int delayMultiplier)
    {
        Shattering = true;
        ShatterCountdown = delayMultiplier * BreakDelayAdd;

        pitchMultiplier = Mathf.Pow(9f / 8, delayMultiplier);
        mover.Locked = true;
    }
}
