using UnityEngine;

public class RadialPosition
{
    public readonly int Lane;
    public readonly int Position;

    public RadialPosition(int lane, int position)
    {
        Lane = lane % BoardController.NUM_LANES;
        if (Lane < 0) Lane += BoardController.NUM_LANES;
        Position = Mathf.Clamp(position, 0, BoardController.NUM_SPACES);
    }
}