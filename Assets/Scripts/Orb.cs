using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Orb : MonoBehaviour
{
    // Configuration
    public bool Shoot;
    public bool BounceBack;
    public float BreakDelayAdd;
    public PitchVariance Pitch;

    public Vector2 StraightBump;
    public Vector2 DiagonalBump;

    // Runtime
    public int Type;
    AudioSource audioSource;
    public bool Shattering;
    public float ShatterCountdown;
    BoardMover mover;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Pitch = GetComponent<PitchVariance>();
        mover = GetComponent<BoardMover>();
    }

    void Update()
    {
        if (Shattering)
        {
            ShatterCountdown -= Time.deltaTime;
            if (ShatterCountdown <= 0)
            {
                BoardController.Instance.RemoveMover(mover);
                Destroy(gameObject);
            }
        }

        //if (Shot)
        //{
        //    JustShot = false;
        //    if (Position.Position < BoardController.NUM_SPACES - 1)
        //    {
        //        audioSource.pitch = Pitch.GetRandomPitch();
        //        audioSource.Play();
        //    }
        //    BounceBack = true;
        //}
        //else if (BounceBack)
        //{
        //    BounceBack = false;
        //    MovementType = MovementType.Sliding;
        //}
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
    }
}
