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
    public Orb OrbPrefab;

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

    public Orb CreateNewOrb(int type, RadialPosition position, bool logical = false)
    {
        var newOrb = Instantiate(OrbPrefab);
        if (position != null)
        {
            BoardController.Instance.AddObject(newOrb.gameObject, position);
            newOrb.Position = position;
            newOrb.transform.position = BoardController.Instance.GetPosition(newOrb.Position, false);
        }
        newOrb.Type = type;
        var info = EnemyInfo[type];
        if (!logical)
        {
            newOrb.GetComponent<SpriteRenderer>().sprite = info.Sprite;
        }

        return newOrb;
    }
}

[Serializable]
public struct EnemyInfo
{
    public Sprite Sprite;
    // TODO: Sound
}