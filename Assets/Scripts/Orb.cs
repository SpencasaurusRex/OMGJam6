using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Orb : MonoBehaviour
{
    // Configuration
    public float MovementSharpness;
    public float Speed;
    public MovementType MovementType = MovementType.Sliding;
    public bool JustShot;

    // Runtime
    public int Type;
    public RadialPosition Position;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Position == null) return;
        var targetPosition = BoardController.Instance.GetPosition(Position, JustShot);

        if (MovementType == MovementType.Sliding)
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-MovementSharpness * Time.deltaTime));
        }
        else if (MovementType == MovementType.Shooting)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
        }

        if (((Vector2)transform.localPosition - targetPosition).magnitude <= .01f)
        {
            if (JustShot)
            {
                JustShot = false;
                if (Position.Position < BoardController.NUM_SPACES - 1)
                {
                    audioSource.Play();
                }
            }
        }
    }
}
public enum MovementType
{
    Sliding,
    Shooting
}