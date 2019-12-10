using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    // TODO
    //public float Difficulty;

    // Configuration
    public Orb EnemyPrefab;
    public float StartingInterval;
    public int LaneIndex;

    // Runtime;
    float Cooldown;
    float Interval;
    Queue<Orb> Enemies = new Queue<Orb>();

    void Awake()
    {
        Cooldown = Interval = StartingInterval;
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
        var enemiesArray = Enemies.ToArray();
        
        foreach (var enemy in enemiesArray)
        {
            var newPosition = new RadialPosition(enemy.Position.Lane, enemy.Position.Position - 1);
            if (BoardController.Instance.TryMove(enemy.gameObject, enemy.Position, newPosition))
            {
                enemy.Position = newPosition;
            }
        }

        var movePosition = new RadialPosition(LaneIndex, BoardController.NUM_SPACES - 2);
        var spawnPosition = new RadialPosition(LaneIndex, BoardController.NUM_SPACES - 1);

        if (BoardController.Instance.GetObject(spawnPosition) != null) return;

        var newOrb = Instantiate(EnemyPrefab);
        Enemies.Enqueue(newOrb);

        int type = GetNextOrbType();

        newOrb.Position = spawnPosition;
        BoardController.Instance.AddObject(newOrb.gameObject, spawnPosition);
        newOrb.transform.position = BoardController.Instance.GetPosition(newOrb.Position);
        newOrb.Type = type;
        var info = EnemyController.Instance.EnemyInfo[type];
        if (LaneIndex % 2 == 0)
        {
            newOrb.GetComponent<SpriteRenderer>().sprite = info.CardinalSprite;
        }
        else
        {
            newOrb.GetComponent<SpriteRenderer>().sprite = info.DiagonalSprite;
        }

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