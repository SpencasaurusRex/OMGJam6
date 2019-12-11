using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class Lane : MonoBehaviour
{
    // TODO
    //public float Difficulty;

    // Configuration
    public Orb EnemyPrefab;
    public float StartingInterval;
    public int LaneIndex;
    public BoardController BoardController;

    // Runtime;
    [NonSerialized]
    public Vector2[] Spaces = new Vector2[BoardController.NUM_SPACES];
    [NonSerialized]
    public GameObject[] Objects = new GameObject[BoardController.NUM_SPACES];
    float Cooldown;
    float Interval;

    void Awake()
    {
        Cooldown = Interval = StartingInterval;

        List<Vector2> positions = new List<Vector2>();
        float laneRotation = LaneIndex / 2 * 90;
        for (int position = 0; position < BoardController.NUM_SPACES; position++)
        {
            Vector2 newPosition;
            if (LaneIndex % 2 == 0)
            {
                Vector2 extra = position == BoardController.NUM_SPACES - 1
                    ? BoardController.LastLaneDifferenceStraight
                    : Vector2.zero;
                newPosition =
                    (BoardController.InitialStraight + BoardController.OffsetStraight * position + extra).Rotate(
                        laneRotation);
            }
            else
            {
                Vector2 extra = position == BoardController.NUM_SPACES - 1
                    ? BoardController.LastLaneDifferenceDiagonal
                    : Vector2.zero;
                newPosition =
                    (BoardController.InitialDiagonal + BoardController.OffsetDiagonal * position + extra).Rotate(
                        laneRotation);
            }
            Spaces[position] = newPosition;
        }
    }

    void Update()
    {
        Cooldown -= Time.deltaTime;
        if (Cooldown <= 0)
        {
            Cooldown = Interval;
            SpawnEnemies();
        } 
    }

    void SpawnEnemies()
    {
        // Move all orbs forward
        foreach (var obj in Objects)
        {
            if (obj == null) continue;
            if (obj.TryGetComponent<Orb>(out var orb))
            {
                var newPosition = new RadialPosition(orb.Position.Lane, orb.Position.Position - 1);
                if (BoardController.Instance.TryMove(orb.gameObject, orb.Position, newPosition))
                {
                    orb.Position = newPosition;
                }
            }
        }

        var movePosition = new RadialPosition(LaneIndex, BoardController.NUM_SPACES - 2);
        var spawnPosition = new RadialPosition(LaneIndex, BoardController.NUM_SPACES - 1);

        if (BoardController.Instance.GetObject(spawnPosition) != null) return;

        int type = GetNextOrbType();
        var newOrb = EnemyController.Instance.CreateNewOrb(type, spawnPosition); 
        Objects[BoardController.NUM_SPACES - 1] = newOrb.gameObject;

        if (BoardController.Instance.TryMove(newOrb.gameObject, spawnPosition, movePosition))
        {
            newOrb.Position = movePosition;
        }
    }

    int GetNextOrbType()
    {
        return Random.Range(0, EnemyController.Instance.AvailableTypes);
    }
}