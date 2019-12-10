using UnityEngine;

public class Player : MonoBehaviour
{
    // Configuration
    public KeyCode Clockwise;
    public KeyCode CounterClockwise;
    public float WaitTime;
    public float MovementSharpness;

    // Runtime
    public RadialPosition Position = new RadialPosition(0, 0);
    public float Cooldown;

    void Update()
    {
        Cooldown -= Time.deltaTime;
        
        // Change transform
        var targetPosition = BoardController.Instance.GetPosition(Position);
        transform.position = Vector2.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-MovementSharpness * Time.deltaTime));
        
        if (Cooldown > 0) return;

        // Input
        int delta = 0;
        if (Input.GetKeyDown(Clockwise)) delta--;
        if (Input.GetKeyDown(CounterClockwise)) delta++;

        if (delta != 0)
        {
            var newPosition = new RadialPosition(Position.Lane + delta, 0);
            var moved = BoardController.Instance.TryMove(gameObject, Position, newPosition);
            if (moved)
            {
                Position = newPosition;
                Cooldown = WaitTime;
            }
        }
    }
}
