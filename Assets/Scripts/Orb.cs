using UnityEngine;

public class Orb : MonoBehaviour
{
    // Configuration
    public float MovementSharpness;

    // Runtime
    public int Type;
    public RadialPosition Position;

    void Update()
    {
        var targetPosition = BoardController.Instance.GetPosition(Position);
        transform.position = Vector2.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-MovementSharpness * Time.deltaTime));
    }
}