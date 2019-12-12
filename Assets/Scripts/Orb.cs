using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Orb : MonoBehaviour
{
    // Configuration
    public float MovementSharpness;
    public float Speed;
    public MovementType MovementType = MovementType.Sliding;
    public bool JustShot;
    public bool BounceBack;
    public float BreakDelayAdd;
    public PitchVariance Pitch;

    // Runtime
    public int Type;
    public RadialPosition Position;
    AudioSource audioSource;
    public bool Shattering;
    public float ShatterCountdown;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Pitch = GetComponent<PitchVariance>();
    }

    void Update()
    {
        if (Position == null) return;
        var targetPosition = BoardController.Instance.GetPosition(Position, JustShot);

        if (Shattering)
        {
            ShatterCountdown -= Time.deltaTime;
            if (ShatterCountdown <= 0)
            {
                BoardController.Instance.Lanes[Position.Lane].Objects[Position.Position] = null;
                Destroy(gameObject);
            }
        }

        if (MovementType == MovementType.Sliding)
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-MovementSharpness * Time.deltaTime));
        }
        else if (MovementType == MovementType.Shooting)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime * (BounceBack ? .3f : 1));
        }

        if (((Vector2)transform.localPosition - targetPosition).magnitude <= .01f)
        {
            if (JustShot)
            {
                JustShot = false;
                if (Position.Position < BoardController.NUM_SPACES - 1)
                {
                    audioSource.pitch = Pitch.GetRandomPitch();
                    audioSource.Play();
                }
                BounceBack = true;
            }
            else if (BounceBack)
            {
                BounceBack = false;
                MovementType = MovementType.Sliding;
            }
        }
    }

    public void Shatter(int delayMultiplier)
    {
        Shattering = true;
        ShatterCountdown = delayMultiplier * BreakDelayAdd;
    }
}
public enum MovementType
{
    Sliding,
    Shooting
}