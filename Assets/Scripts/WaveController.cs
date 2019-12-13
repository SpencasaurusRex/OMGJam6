using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    // Configuration
    public int[] ActivateTimes;
    public float LaneSpawnTime;

    // Runtime
    public int AvailableTypes = 3;
    float timer;
    List<float> laneCountDowns;
    List<int> lastLaneType;
    public static WaveController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        laneCountDowns = new List<float>();
        lastLaneType = new List<int>();
        foreach (int i in ActivateTimes)
        {
            laneCountDowns.Add(i);
            lastLaneType.Add(0);
        }
    }

    void Update()
    {
        for (int lane = 0; lane < ActivateTimes.Length; lane++)
        {
            var position = new RadialPosition(lane, BoardController.NUM_SPACES - 1);
            laneCountDowns[lane] -= Time.deltaTime;
            if (laneCountDowns[lane] <= 0)
            {
                laneCountDowns[lane] = LaneSpawnTime;
                SpawnOrb(position);
            }
        }
    }

    void SpawnOrb(RadialPosition pos)
    {
        if (BoardController.Instance.GetMover(pos) == null)
        {
            Factory.Instance.CreateLaneOrb(pos.Lane, GetNextOrbType(pos.Lane));
        }
        for (int i = 1; i < BoardController.NUM_SPACES; i++)
        {
            var movePos = new RadialPosition(pos.Lane, i);
            var mover = BoardController.Instance.GetMover(movePos);
            if (mover == null) continue;
            BoardController.Instance.TryMove(mover, new RadialPosition(movePos.Lane, movePos.Position - 1));
        }
    }

    int GetNextOrbType(int lane)
    {
        return Random.Range(0, AvailableTypes - 1);
    }
}