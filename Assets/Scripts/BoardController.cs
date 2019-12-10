using System;
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

    // Runtime
    public List<BoardLane> Lanes = new List<BoardLane>();

    public static BoardController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        for (int laneIndex = 0; laneIndex < NUM_LANES; laneIndex++)
        {
            var lane = new BoardLane();
            Lanes.Add(lane);
            float laneRotation = laneIndex / 2 * 90;
            for (int position = 0; position < NUM_SPACES; position++)
            {
                if (laneIndex % 2 == 0)
                {
                    Vector2 extra = position == NUM_SPACES - 1 ? LastLaneDifferenceStraight : Vector2.zero;
                    Vector2 newPosition = (InitialStraight + OffsetStraight * position + extra).Rotate(laneRotation);
                    lane.Add(newPosition);
                }
                else
                {
                    Vector2 extra = position == NUM_SPACES - 1 ? LastLaneDifferenceDiagonal : Vector2.zero;
                    Vector2 newPosition = (InitialDiagonal + OffsetDiagonal * position + extra).Rotate(laneRotation);
                    lane.Add(newPosition);
                }
            }
        }
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

    public Vector2 GetPosition(RadialPosition pos)
    {
        return Lanes[pos.Lane].Spaces[pos.Position];
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

[Serializable]
public class BoardLane
{
    public void Add(Vector2 position)
    {
        Spaces.Add(position);
        Objects.Add(null);
    }

    public List<Vector2> Spaces = new List<Vector2>();
    public List<GameObject> Objects = new List<GameObject>();
}
