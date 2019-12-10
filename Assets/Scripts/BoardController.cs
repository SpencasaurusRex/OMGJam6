using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public const int NUM_LANES = 8;
    public const int NUM_SPACES = 8;

    // Configuration
    public Vector2 InitialStraight;
    public Vector2 OffsetStraight;

    public Vector2 InitialDiagonal;
    public Vector2 OffsetDiagonal;
    public int LaneLength;

    // Runtime
    public List<BoardLane> Lanes = new List<BoardLane>();

    public static BoardController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        for (int laneIndex = 0; laneIndex < 8; laneIndex++)
        {
            var lane = new BoardLane();
            Lanes.Add(lane);
            float laneRotation = laneIndex / 2 * 90;
            for (int position = 0; position < LaneLength; position++)
            {
                if (laneIndex % 2 == 0)
                {
                    Vector2 newPosition = (InitialStraight + OffsetStraight * position).Rotate(laneRotation);
                    lane.Add(newPosition);
                }
                else
                {
                    Vector2 newPosition = (InitialDiagonal + OffsetDiagonal * position).Rotate(laneRotation);
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

    public Vector2 GetPosition(RadialPosition pos) => Lanes[pos.Lane].Spaces[pos.Position];

    public bool TryMove(GameObject obj, RadialPosition from, RadialPosition to)
    {
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
