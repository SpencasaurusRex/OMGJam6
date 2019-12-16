using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;

public class Player : MonoBehaviour
{
    // Configuration
    public int QueueSize = 5;
    public Laser LaserPrefab;
    public AudioClip MovementClip;
    public AudioClip LaserClip;
    public AudioClip BlockedClip;
    public AudioClip SuperShotClip;
    public AudioSource SuperShotDrone;
    public float BulletRechargeTime;
    public List<Sprite> PlayerSprites;

    public float LaserVolume = 1.3f;
    public float MovementVolume = 1.3f;
    public float BlockedVolume = 1f;

    // Runtime
    PitchVariance pitchVariance;
    public int Charge;
    public int BulletsLeft;
    public float BulletRechargeAmount;
    BoardMover boardMover;
    SpriteRenderer sr;

    public bool OrbShotEnabled;
    public bool LaserEnabled;
    public bool SwapEnabled;
    public bool SupershotEnabled;
    public bool MovementEnabled;

    Queue<int> ShootOrbs = new Queue<int>();
    public int StoredType;

    public delegate void ShootOrb(int[] newTypes);
    public event ShootOrb OnShootOrb;

    public delegate void SwapStore(int newStoreType, int newFrontType, bool skipAntimation);
    public event SwapStore OnSwapStore;

    public delegate void ShootLaser();
    public event ShootLaser OnShootLaser;

    public delegate void Move(RadialPosition to);
    public event Move OnMove;

    public delegate void SuperShotCharged();
    public event SuperShotCharged OnSuperShotCharged;

    void Awake()
    {
        boardMover = GetComponent<BoardMover>();
        pitchVariance = GetComponent<PitchVariance>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup()
    {
        SetupOrbs(true, 0);
        boardMover.enabled = true;

        var startingPos = new RadialPosition(2, 0);
        BoardController.Instance.AddMover(GetComponent<BoardMover>(), startingPos);
        sr.sprite = PlayerSprites[startingPos.Lane];

        transform.position = BoardController.Instance.GetPosition(startingPos);

        BulletsLeft = 4;
        BulletRechargeAmount = 0;
    }

    public void SetupOrbs(bool random, int type)
    {
        ShootOrbs.Clear();
        for (int i = 0; i < QueueSize; i++)
        {
            int nextType = random ? GetNextType() : type;
            ShootOrbs.Enqueue(nextType);
        }

        OnShootOrb?.Invoke(ShootOrbs.ToArray());
        OnSwapStore?.Invoke(StoredType, ShootOrbs.Peek(), true);
    }

    public void CallOnSwapStore()
    {
        OnSwapStore?.Invoke(StoredType, ShootOrbs.Peek(), true);
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

        var position = boardMover.Position;

        // Input
        if (MovementEnabled) KeyboardMovement();
        //KeyboardMovement2();

        var pos = BoardController.Instance.GetMoverPosition(boardMover);
        int lastEmptySpace = BoardController.Instance.GetLastEmptySpace(pos.Lane);

        if (Input.GetMouseButtonDown(0) && OrbShotEnabled && !EventSystem.current.IsPointerOverGameObject())
        {
            if (lastEmptySpace == 0)
            {
                Factory.Instance.PlaySound(BlockedClip, pitchVariance.GetRandomPitch(), BlockedVolume);
                return;
            }

            // Shoot orb
            int type = ShootOrbs.Dequeue();
            Factory.Instance.CreateLaunchOrb(pos.Lane, transform.position, type, lastEmptySpace);

            ShootOrbs.Enqueue(GetNextType());
            OnShootOrb?.Invoke(ShootOrbs.ToArray());
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (LaserEnabled && BulletsLeft > 0)
            {
                BulletsLeft--;

                var spawnPosition = BoardController.Instance.GetPosition(position);
                float degrees = position.Lane / 8f * 360;
                var laser = Instantiate(LaserPrefab, spawnPosition, Quaternion.Euler(0, 0, degrees));

                laser.player = this;
                laser.TargetPosition = new RadialPosition(position.Lane, lastEmptySpace + 1);
                Factory.Instance.PlaySound(LaserClip, pitchVariance.GetRandomPitch(), LaserVolume);

                OnShootLaser?.Invoke();
            }
            else
            {
                Factory.Instance.PlaySound(BlockedClip, 1.3f, 0.5f);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space) && Charge == 8 && SupershotEnabled)
        {
            SuperShotDrone.Stop();
            Charge = 0;
            BulletsLeft = Mathf.Min(BulletsLeft + 1, 4);

            Factory.Instance.CreateSuperShotLaser(position.Lane);
            Factory.Instance.CreateCampfireBlast();
            Factory.Instance.PlaySound(SuperShotClip, 1, 0.5f);
            var movers = BoardController.Instance.GetLane(position.Lane).Where(x => x != null);

            int orbCount = 0;
            foreach (var mover in movers)
            {
                if (mover.TryGetComponent<Orb>(out var orb))
                {
                    orb.Shatter(0, false);
                    ScoreController.Instance.OrbBreak(orbCount++);
                }
            }

            for (int i = 0; i < BoardController.NUM_LANES; i++)
            {
                if (i == position.Lane) continue;
                var mover = BoardController.Instance.GetMover(new RadialPosition(i, 0));
                if (mover != null && mover.TryGetComponent<Orb>(out var orb))
                {
                    orb.Shatter(0, false);
                    ScoreController.Instance.OrbBreak(0);
                }

                mover = BoardController.Instance.GetMover(new RadialPosition(i, 1));
                if (mover != null && mover.TryGetComponent(out orb))
                {
                    orb.CheckForChain();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && SwapEnabled)
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
        return Random.Range(0, WaveController.Instance.AvailableTypes);
    }

    public void ChargeGun(int chainLength)
    {
        Charge = Mathf.Min(Charge + Mathf.Max(chainLength - 2, 0), 8);
        if (Charge == 8)
        {
            SuperShotDrone.Play();
            OnSuperShotCharged?.Invoke();
        }
    }

    void KeyboardMovement()
    {
        var position = boardMover.Position;
        int delta = 0;
        if (Input.GetKeyDown(KeyCode.D)) delta--;
        if (Input.GetKeyDown(KeyCode.A)) delta++;
        if (delta != 0)
        {
            var newPosition = new RadialPosition(position.Lane + delta, 0);
            if (BoardController.Instance.TryMove(boardMover, newPosition))
            {
                Factory.Instance.PlaySound(MovementClip, pitchVariance.GetRandomPitch(), MovementVolume);
                sr.sprite = PlayerSprites[newPosition.Lane];
                OnMove?.Invoke(newPosition);
            }
            else
            {
                Factory.Instance.PlaySound(BlockedClip, pitchVariance.GetRandomPitch(), BlockedVolume);
            }
        }
    }

}
