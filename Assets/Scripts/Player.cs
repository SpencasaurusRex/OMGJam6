using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Configuration
    public KeyCode Clockwise;
    public KeyCode CounterClockwise;
    public KeyCode LaunchOrb;
    public KeyCode Laser;
    public KeyCode Swap;
    public KeyCode SuperShot;
    public int QueueSize = 5;
    public Laser LaserPrefab;
    public AudioSource LaserSource;
    public AudioSource BlockedSource;
    public float BulletRechargeTime;
    public List<Sprite> PlayerSprites;

    // Runtime
    PitchVariance pitchVariance;
    public int Charge;
    public int BulletsLeft;
    public float BulletRechargeAmount;
    AudioSource movementSource;
    BoardMover boardMover;
    SpriteRenderer sr;

    Queue<int> ShootOrbs = new Queue<int>();
    int StoredType;

    public delegate void ShootOrb(int[] newTypes);
    public event ShootOrb OnShootOrb;

    public delegate void SwapStore(int newStoreType, int newFrontType, bool skipAntimation);
    public event SwapStore OnSwapStore;

    void Awake()
    {
        boardMover = GetComponent<BoardMover>();
        movementSource = GetComponent<AudioSource>();
        pitchVariance = GetComponent<PitchVariance>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        BoardController.Instance.AddMover(GetComponent<BoardMover>(), new RadialPosition(0, 0));
        sr.sprite = PlayerSprites[0];

        for (int i = 0; i < QueueSize; i++)
        {
            ShootOrbs.Enqueue(GetNextType());
        }

        OnShootOrb?.Invoke(ShootOrbs.ToArray());
        OnSwapStore?.Invoke(StoredType, ShootOrbs.Peek(), true);

        BulletsLeft = 4;

    }

    void Update()
    {
        if (BulletsLeft < 4)
        {
            BulletRechargeAmount += Time.deltaTime / BulletRechargeTime;
            if (BulletRechargeAmount > 1)
            {
                BulletsLeft++;
                BulletRechargeAmount = 0;
            }
        }

        // Input
        int delta = 0;
        if (Input.GetKeyDown(Clockwise)) delta--;
        if (Input.GetKeyDown(CounterClockwise)) delta++;

        var position = boardMover.Position;

        if (delta != 0)
        {
            var newPosition = new RadialPosition(position.Lane + delta, 0);
            if (BoardController.Instance.TryMove(boardMover, newPosition))
            {
                movementSource.pitch = pitchVariance.GetRandomPitch();
                movementSource.Play();
                sr.sprite = PlayerSprites[newPosition.Lane];
            }
            else
            {
                BlockedSource.pitch = pitchVariance.GetRandomPitch();
                BlockedSource.Play();
            }
        }

        var pos = BoardController.Instance.GetMoverPosition(boardMover);
        int lastEmptySpace = BoardController.Instance.GetLastEmptySpace(pos.Lane);

        if (Input.GetKeyDown(LaunchOrb))
        {
            if (lastEmptySpace == 0)
            {
                BlockedSource.pitch = pitchVariance.GetRandomPitch();
                BlockedSource.Play();
                return;
            }

            // Shoot orb
            int type = ShootOrbs.Dequeue();
            Factory.Instance.CreateLaunchOrb(pos.Lane, transform.position, type, lastEmptySpace);

            ShootOrbs.Enqueue(GetNextType());
            OnShootOrb?.Invoke(ShootOrbs.ToArray());
        }
        else if (Input.GetKeyDown(Laser))
        {
            if (BulletsLeft > 0)
            {
                BulletsLeft--;

                var spawnPosition = BoardController.Instance.GetPosition(position);
                float degrees = position.Lane / 8f * 360;
                var laser = Instantiate(LaserPrefab, spawnPosition, Quaternion.Euler(0, 0, degrees));

                laser.player = this;
                laser.TargetPosition = new RadialPosition(position.Lane, lastEmptySpace + 1);
                LaserSource.pitch = pitchVariance.GetRandomPitch();
                LaserSource.Play();
            }
        }
        else if (Input.GetKeyDown(SuperShot) && Charge == 8)
        {
            Charge = 0;
            var movers = BoardController.Instance.GetLane(position.Lane).Where(x => x != null);
            
            foreach (var mover in movers)
            {
                if (mover.TryGetComponent<Orb>(out var orb))
                {
                    orb.Shatter(0);
                }
            }

            for (int i = 0; i < BoardController.NUM_LANES; i++)
            {
                var mover = BoardController.Instance.GetMover(new RadialPosition(i, 0));
                if (mover != null && mover.TryGetComponent<Orb>(out var orb))
                {
                    orb.Shatter(0);
                }
            }
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

            OnSwapStore?.Invoke(oldFront, StoredType, false);
            
            StoredType = oldFront;
        }
    }

    public int GetNextType()
    {
        return Random.Range(0, WaveController.Instance.AvailableTypes - 1);
    }

    public void ChargeGun(int chainLength)
    {
        Charge = Mathf.Min(Charge + Mathf.Max(chainLength - 2, 0), 8);
    }
}
