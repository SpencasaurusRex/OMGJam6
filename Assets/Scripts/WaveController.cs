using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WaveController : MonoBehaviour
{
    // Configuration
    public int[] ActivateTimes;
    public float LaneSpawnTime;

    public List<Animator> WobbleAnimators;
    public List<SpriteRenderer> WobbleSprites;

    public SpriteRenderer OrbSpawnPrefab;

    // Runtime
    float particleSpawnLength = 0.3f;
    public int AvailableTypes = 3;
    float timer;
    List<float> laneCountDowns;
    List<int> lastLaneType;
    List<int> nextOrbTypes;
    List<bool> spawnedParticleCreation;
    List<bool> FirstEnabled;

    public int LanesLeft;
    public int SpacesLeft;

    public static WaveController Instance { get; private set; }

    const int TOTAL_SPACES = (BoardController.NUM_LANES - 1) * BoardController.NUM_SPACES;

    void Awake()
    {
        Instance = this;
        Setup();
    }

    public void Setup()
    {
        laneCountDowns = new List<float>();
        lastLaneType = new List<int>();
        FirstEnabled = new List<bool>();

        foreach (int i in ActivateTimes)
        {
            laneCountDowns.Add(i);
            lastLaneType.Add(0);
        }

        nextOrbTypes = new List<int>();
        spawnedParticleCreation = new List<bool>();
        for (int lane = 0; lane < BoardController.NUM_LANES; lane++)
        {
            nextOrbTypes.Add(GetNextOrbType(lane));
            spawnedParticleCreation.Add(false);
            FirstEnabled.Add(false);
        }

        foreach (var wobbleSprite in WobbleSprites)
        {
            wobbleSprite.color = new Color(1, 1, 1, 0);
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
            if (!FirstEnabled[lane] && laneCountDowns[lane] <= 5 || FirstEnabled[lane])
            {
                var c = Factory.Instance.OrbColors[nextOrbTypes[lane]];
                if (BoardController.Instance.GetMover(position) != null)
                {
                    WobbleSprites[lane].color = new Color(c.r, c.g, c.b, 0); 
                }
                else
                {
                    WobbleSprites[lane].color = new Color(c.r, c.g, c.b, 1); 
                }
            }

            if (laneCountDowns[lane] <= particleSpawnLength && !spawnedParticleCreation[lane] && BoardController.Instance.GetMover(position) == null)
            {
                var orbSpawn = Instantiate(OrbSpawnPrefab, WobbleSprites[lane].transform.localPosition, Quaternion.identity);
                orbSpawn.color = Factory.Instance.OrbColors[nextOrbTypes[lane]];
                spawnedParticleCreation[lane] = true;
            }
            if (laneCountDowns[lane] <= 0)
            {
                laneCountDowns[lane] = LaneSpawnTime * ((float)LanesLeft / (BoardController.NUM_LANES - 1));
                SpawnOrb(position, lane);
                nextOrbTypes[lane] = GetNextOrbType(lane);
                spawnedParticleCreation[lane] = false;
                WobbleSprites[lane].color = Factory.Instance.OrbColors[nextOrbTypes[lane]];
            }
        }

        LightController.Instance.LightAmount(SpacesLeft / (float)TOTAL_SPACES);

        if (SpacesLeft <= 1)
        {
            GameController.Instance.EndGame();
        }
    }

    void SpawnOrb(RadialPosition pos, int lane)
    {
        if (BoardController.Instance.GetMover(pos) == null)
        {
            Factory.Instance.CreateLaneOrb(pos.Lane, nextOrbTypes[lane]);
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
        return Random.Range(0, AvailableTypes);
    }
}