using UnityEngine;

public class BoardMover : MonoBehaviour
{
    public delegate void Remove();
    public event Remove OnRemove;

    public delegate void CloseEnough();
    public event CloseEnough OnCloseEnough;

    public float LerpSharpness;
    public float LinearSpeed;

    public Vector2 Offset;
    public MovementType MovementType = MovementType.Lerp;

    public bool Locked;

    public RadialPosition Position => BoardController.Instance.GetMoverPosition(this);

    void Update()
    {
        var target = BoardController.Instance.GetPosition(this) + Offset;

        if (MovementType == MovementType.Lerp)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, target, 1f - Mathf.Exp(-LerpSharpness * Time.deltaTime));
        }
        else
        {
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, target, LinearSpeed * Time.deltaTime);
        }

        if ((target - (Vector2)transform.localPosition).magnitude < 0.01f)
        {
            OnCloseEnough?.Invoke();
        }
    }

    public void CallRemove()
    {
        OnRemove?.Invoke();
    }
}

public enum MovementType
{
    Lerp,
    Linear
}