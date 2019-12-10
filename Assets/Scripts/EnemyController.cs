using System;
using System.Collections.Generic;
using UnityEngine;
public class EnemyController : MonoBehaviour
{
    BoardController board;

    // Configuration
    public List<int> StartingLanes;
    public List<EnemyInfo> EnemyInfo;
    public int AvailableTypes;

    // Runtime
    public static EnemyController Instance { get; private set; }

    void Awake()
    {
        board = GetComponent<BoardController>();
        Instance = this;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

[Serializable]
public struct EnemyInfo
{
    public Sprite Sprite;
    // TODO: Sound
}