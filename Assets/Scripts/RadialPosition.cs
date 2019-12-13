using System;
using UnityEngine;

[Serializable]
public class RadialPosition : IEquatable<RadialPosition>
{
    public int Lane;
    public int Position;

    public int Index => BoardController.NUM_SPACES * Lane + Position;

    public RadialPosition(int lane, int position)
    {
        Lane = lane % BoardController.NUM_LANES;
        if (Lane < 0) Lane += BoardController.NUM_LANES;
        Position = Mathf.Clamp(position, 0, BoardController.NUM_SPACES - 1);
    }

    public bool Equals(RadialPosition other)
    {
        if (ReferenceEquals(null, other)) return false;
        return Lane == other.Lane && Position == other.Position;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Lane * 397) ^ Position;
        }
    }
}