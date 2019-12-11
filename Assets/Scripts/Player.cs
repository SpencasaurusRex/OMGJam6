using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Configuration
    public KeyCode Clockwise;
    public KeyCode CounterClockwise;
    public KeyCode LaunchOrb;
    public KeyCode Laser;
    public float WaitTime;
    public float MovementSharpness;
    public AudioClip Movement;
    public AudioClip Blocked;
    public AudioClip LaserSound;
    public int QueueSize = 5;
    public Laser LaserPrefab;

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

        Lane lane = BoardController.Instance.Lanes[Position.Lane];
        int lastEmptySpace = lane.GetLastEmptySpace();

        if (Input.GetKeyDown(LaunchOrb))
        {
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
            orb.GetComponent<BoxCollider2D>().enabled = true;
            orb.JustShot = true;
            orb.Position = new RadialPosition(Position.Lane, lastEmptySpace);
            BoardController.Instance.AddObject(orb.gameObject, orb.Position);

            CreateOrb();
        }
        else if (Input.GetKeyDown(Laser))
        {
            var spawnPosition = BoardController.Instance.GetPosition(Position);
            float degrees = Position.Lane / 8f * 360;
            var laser = Instantiate(LaserPrefab, spawnPosition, Quaternion.Euler(0, 0, degrees));

            laser.TargetPosition = new RadialPosition(Position.Lane, lastEmptySpace + 1);
            audioSource.PlayOneShot(LaserSound);

            laser.OnLaserHit += lane.LaserHit;
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
