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

    public BoardMover Blocker;

    // Runtime
    public static BoardController Instance { get; private set; }
    Vector2[] positions;

    BoardMover[] movers;
    Dictionary<BoardMover, RadialPosition> positionLookup;


    void Awake()
    {
        Instance = this;

        positions = new Vector2[NUM_SPACES * NUM_LANES];
        movers = new BoardMover[NUM_SPACES * NUM_LANES];
        positionLookup = new Dictionary<BoardMover, RadialPosition>();

        for (int lane = 0; lane < NUM_LANES; lane++)
        {
            bool even = lane % 2 == 0;
            Vector2 pos = even ? InitialStraight : InitialDiagonal;
            float laneAngle = (lane / 2) * 90f;
            for (int position = 0; position < NUM_SPACES; position++)
            {
                positions[lane * NUM_SPACES + position] = pos.Rotate(laneAngle);
                pos += even ? OffsetStraight : OffsetDiagonal;

                if (position == NUM_SPACES - 2)
                {
                    pos += even ? LastLaneDifferenceStraight : LastLaneDifferenceDiagonal;
                }
            }
        }

    }

    public void AddMover(BoardMover mover, RadialPosition pos)
    {
        if (positionLookup.ContainsKey(mover)) throw new Exception("Mover already on board");

        positionLookup.Add(mover, pos);
        movers[pos.Index] = mover;
    }

    public bool TryMove(BoardMover mover, RadialPosition to)
    {
        if (mover.Locked) return false;
        if (GetMover(to) == null)
        {
            movers[GetMoverPosition(mover).Index] = null;
            positionLookup.Remove(mover);
            AddMover(mover, to);
            mover.CallMove();
            return true;
        }
        return false;
    }

    public void RemoveMover(BoardMover mover)
    {
        if (!positionLookup.ContainsKey(mover)) throw new Exception("Mover not on board");

        movers[GetMoverPosition(mover).Index] = null;
        positionLookup.Remove(mover);

        mover.CallRemove();
    }

    public BoardMover GetMover(RadialPosition pos) => movers[pos.Index];

    public RadialPosition GetMoverPosition(BoardMover mover) => positionLookup.TryGetValue(mover, out var pos) ? pos : null;

    public Vector2 GetPosition(RadialPosition pos) => positions[pos.Index];

    public Vector2 GetPosition(BoardMover mover) => GetPosition(GetMoverPosition(mover));

    public int GetLastEmptySpace(int lane)
    {
        int startIndex = new RadialPosition(lane, 0).Index;

        for (int i = 1; i < NUM_SPACES; i++)
        {
            if (movers[startIndex + i] != null) return i - 1;
        }

        return NUM_SPACES - 1;
    }

    public BoardMover[] GetLane(int lane)
    {
        BoardMover[] results = new BoardMover[BoardController.NUM_SPACES];
        for (int i = 0; i < NUM_SPACES; i++)
        {
            results[i] = movers[lane * NUM_SPACES + i];
        }

        return results;
    }

    public void Clear()
    {
        for (int i = 0; i < movers.Length; i++)
        {
            if (movers[i] != null && movers[i].TryGetComponent<Orb>(out var orb))
            {
                Destroy(orb.gameObject);
            }
            movers[i] = null;
        }
        positionLookup.Clear();

        // Block lane 6
        for (int pos = 1; pos < NUM_SPACES; pos++)
        {
            movers[6 * NUM_SPACES + pos] = Blocker;
        }
    }
}
