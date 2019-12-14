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

    public int LanesLeft;
    public int SpacesLeft;

    public static WaveController Instance { get; private set; }

    const int TOTAL_SPACES = (BoardController.NUM_LANES - 1) * BoardController.NUM_SPACES;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
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
        SpacesLeft = TOTAL_SPACES;
        LanesLeft = BoardController.NUM_LANES;
        for (int lane = 0; lane < BoardController.NUM_LANES; lane++)
        {
            for (int space = 0; space < BoardController.NUM_SPACES - 1; space++)
            {
                if (BoardController.Instance.GetMover(new RadialPosition(lane, space)) != null)
                {
                    SpacesLeft--;
                    if (space == 0)
                    {
                        LanesLeft--;
                    }
                }
            }
        }

        for (int lane = 0; lane < ActivateTimes.Length; lane++)
        {
            var position = new RadialPosition(lane, BoardController.NUM_SPACES - 1);
            laneCountDowns[lane] -= Time.deltaTime;
            if (laneCountDowns[lane] <= 0)
            {
                laneCountDowns[lane] = LaneSpawnTime * ((float)LanesLeft / (BoardController.NUM_LANES - 1));
                SpawnOrb(position);
            }
        }

        if (SpacesLeft <= 1)
        {
            GameController.Instance.EndGame();
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