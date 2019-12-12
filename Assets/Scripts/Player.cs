using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Configuration
    public KeyCode Clockwise;
    public KeyCode CounterClockwise;
    public KeyCode LaunchOrb;
    public KeyCode Laser;
    public KeyCode Swap;
    public float WaitTime;
    public float MovementSharpness;
    public int QueueSize = 5;
    public Laser LaserPrefab;
    public AudioSource LaserSource;
    public AudioSource BlockedSource;

    // Runtime
    PitchVariance pitchVariance;
    public RadialPosition Position = new RadialPosition(0, 0);
    float Cooldown;
    AudioSource movementSource;
    
    Queue<int> ShootOrbs = new Queue<int>();
    int StoredType;

    public delegate void ShootOrb(int[] newTypes);
    public event ShootOrb OnShootOrb;

    public delegate void SwapStore(int newStoreType, int newFrontType);
    public event SwapStore OnSwapStore;

    void Awake()
    {
        movementSource = GetComponent<AudioSource>();
        pitchVariance = GetComponent<PitchVariance>();
    }

    void Start()
    {
        BoardController.Instance.AddObject(gameObject, Position);

        for (int i = 0; i < QueueSize; i++)
        {
            ShootOrbs.Enqueue(GetNextType());
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
                movementSource.pitch = pitchVariance.GetRandomPitch();
                movementSource.Play();
                Position = newPosition;
                Cooldown = WaitTime;
            }
            else
            {
                BlockedSource.pitch = pitchVariance.GetRandomPitch();
                BlockedSource.Play();
            }
        }

        Lane lane = BoardController.Instance.Lanes[Position.Lane];
        int lastEmptySpace = lane.GetLastEmptySpace();

        if (Input.GetKeyDown(LaunchOrb))
        {
            if (lastEmptySpace == -1)
            {
                BlockedSource.pitch = pitchVariance.GetRandomPitch();
                BlockedSource.Play();
                return;
            }

            // Shoot orb
            int type = ShootOrbs.Dequeue();
            var orb = EnemyController.Instance.CreateNewOrb(type, null);
            orb.transform.localPosition = BoardController.Instance.GetPosition(Position, false);
            orb.MovementType = MovementType.Shooting;
            orb.JustShot = true;
            orb.Position = new RadialPosition(Position.Lane, lastEmptySpace);
            BoardController.Instance.AddObject(orb.gameObject, orb.Position);

            ShootOrbs.Enqueue(GetNextType());
            OnShootOrb?.Invoke(ShootOrbs.ToArray());
        }
        else if (Input.GetKeyDown(Laser))
        {
            var spawnPosition = BoardController.Instance.GetPosition(Position);
            float degrees = Position.Lane / 8f * 360;
            var laser = Instantiate(LaserPrefab, spawnPosition, Quaternion.Euler(0, 0, degrees));

            laser.TargetPosition = new RadialPosition(Position.Lane, lastEmptySpace + 1);
            LaserSource.pitch = pitchVariance.GetRandomPitch();
            LaserSource.Play();

            laser.OnLaserHit += lane.LaserHit;
        }

        if (Input.GetKeyDown(Swap))
        {
            int oldFront = ShootOrbs.Peek();

            for (int i = 0; i < ShootOrbs.Count; i++)
            {
                var old = ShootOrbs.Dequeue();
                if (i == 0) ShootOrbs.Enqueue(StoredType);
                else ShootOrbs.Enqueue(old);
            }

            OnSwapStore?.Invoke(oldFront, StoredType);
            
            StoredType = oldFront;
        }
    }

    public int GetNextType()
    {
        return Random.Range(0, EnemyController.Instance.AvailableTypes);
    }

}
