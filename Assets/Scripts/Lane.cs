using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    // TODO
    //public float Difficulty;

    // Configuration
    public Orb EnemyPrefab;
    public float StartingInterval;

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
        var newOrb = Instantiate(EnemyPrefab);
        Enemies.Enqueue(newOrb);
    }
}