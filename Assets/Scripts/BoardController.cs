using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public const int NUM_LANES = 8;
    public const int NUM_SPACES = 9;

    // Configuration
    public Vector2 InitialStraight;
    public Vector2 OffsetStraight;

    public Vector2 InitialDiagonal;
    public Vector2 OffsetDiagonal;

    public Vector2 LastLaneDifferenceStraight;
    public Vector2 LastLaneDifferenceDiagonal;

    public Vector2 BumpStraight;
    public Vector2 BumpDiagonal;

    // Runtime
    public List<Lane> Lanes;

    public static BoardController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetObject(RadialPosition pos)
    {   
        return Lanes[pos.Lane].Objects[pos.Position];
    }

    public void AddObject(GameObject obj, RadialPosition pos)
    {
        Lanes[pos.Lane].Objects[pos.Position] = obj;
    }

    public void RemoveObject(RadialPosition pos)
    {
        Lanes[pos.Lane].Objects[pos.Position] = null;
    }

    public Vector2 GetPosition(RadialPosition pos, bool shouldBump)
    {
        var vec = Lanes[pos.Lane].Spaces[pos.Position];
        if (shouldBump)
        {
            float laneRotation = pos.Lane / 2 * 90;
            var bump = pos.Lane % 2 == 0 ? BumpStraight : BumpDiagonal;
            return vec + bump.Rotate(laneRotation);
        }
        return vec;
    } 

    public bool TryMove(GameObject obj, RadialPosition from, RadialPosition to)
    {
        if (from == to) return false;
        if (GetObject(to) == null)
        {
            RemoveObject(from);
            AddObject(obj, to);
            return true;
        }

        return false;
    }
}
