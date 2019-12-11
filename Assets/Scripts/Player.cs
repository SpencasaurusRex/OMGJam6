using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Configuration
    public KeyCode Clockwise;
    public KeyCode CounterClockwise;
    public KeyCode LaunchOrb;
    public float WaitTime;
    public float MovementSharpness;
    public AudioClip Movement;
    public AudioClip Blocked;
    public int QueueSize = 5;

    // Runtime
    public RadialPosition Position = new RadialPosition(0, 0);
    float Cooldown;
    AudioSource audioSource;
    Queue<Orb> ShootOrbs = new Queue<Orb>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        BoardController.Instance.AddObject(gameObject, Position);

        for (int i = 0; i < QueueSize; i++)
        {
            CreateOrb();
        }
    }

    void Update()
    {
        Cooldown -= Time.deltaTime;
        
        // Change transform
        var targetPosition = BoardController.Instance.GetPosition(Position, false);
        transform.position = Vector2.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-MovementSharpness * Time.deltaTime));
        
        if (Cooldown > 0) return;

        // Input
        int delta = 0;
        if (Input.GetKeyDown(Clockwise)) delta--;
        if (Input.GetKeyDown(CounterClockwise)) delta++;

        if (delta != 0)
        {
            var newPosition = new RadialPosition(Position.Lane + delta, 0);
            var moved = BoardController.Instance.TryMove(gameObject, Position, newPosition);
            if (moved)
            {
                audioSource.PlayOneShot(Movement);
                Position = newPosition;
                Cooldown = WaitTime;
            }
            else
            {
                audioSource.PlayOneShot(Blocked);
            }
        }

        if (Input.GetKeyDown(LaunchOrb))
        {
            Lane lane = BoardController.Instance.Lanes[Position.Lane];
            int lastEmptySpace = -1;
            for (int i = 1; i < lane.Spaces.Length; i++)
            {
                if (lane.Objects[i] == null)
                {
                    lastEmptySpace = i;
                }
                else
                {
                    break;
                }
            }

            if (lastEmptySpace == -1)
            {
                audioSource.PlayOneShot(Blocked);
                return;
            }

            // Shoot orb
            var orb = ShootOrbs.Dequeue();
            orb.transform.localPosition = BoardController.Instance.GetPosition(Position, false);
            var sr = orb.GetComponent<SpriteRenderer>();
            sr.sprite = EnemyController.Instance.EnemyInfo[orb.Type].Sprite;
            orb.JustShot = true;
            orb.Position = new RadialPosition(Position.Lane, lastEmptySpace);
            BoardController.Instance.AddObject(orb.gameObject, orb.Position);

            CreateOrb();
        }
    }

    public int GetNextType()
    {
        return Random.Range(0, EnemyController.Instance.AvailableTypes);
    }

    public void CreateOrb()
    {
        int type = GetNextType();
        var orb = EnemyController.Instance.CreateNewOrb(type, null, true);
        orb.MovementType = MovementType.Shooting;
        ShootOrbs.Enqueue(orb);
    }
}
