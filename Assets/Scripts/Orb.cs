﻿using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Orb : MonoBehaviour
{
    // Configuration
    public bool Shoot;
    public bool BounceBack;
    public float BreakDelayAdd;
    public PitchVariance Pitch;
    public AudioClip BounceSound;
    public AudioClip ShatterSound;

    public Vector2 StraightBump;
    public Vector2 DiagonalBump;

    public GameObject CapPrefab;
    public GameObject SidePrefab;

    // Runtime
    public int Type;
    AudioSource audioSource;
    public bool Shattering;
    public float ShatterCountdown;
    BoardMover mover;
    float pitchMultiplier;
    bool ShatteringAnimation;
    Animator animator;
    GameObject chainIndicator;

    public delegate void Break();
    public event Break OnBreak;
    bool shatterSound = true;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Pitch = GetComponent<PitchVariance>();
        mover = GetComponent<BoardMover>();
        animator = GetComponent<Animator>();

        mover.OnMove += OnMove;
    }

    void Update()
    {
        if (ShatteringAnimation)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            {
                Destroy(gameObject);
            }
        }
        else if (Shattering)
        {
            ShatterCountdown -= Time.deltaTime;
            if (ShatterCountdown <= 0)
            {
                BoardController.Instance.RemoveMover(mover);
                ShatteringAnimation = true;
                animator.enabled = true;
                if (shatterSound) Factory.Instance.PlaySound(ShatterSound, pitchMultiplier);
            }
        }
    }

    public void CloseEnough()
    {
        if (Shoot)
        {
            Shoot = false;
            BounceBack = true;
            if (mover.Position.Position < BoardController.NUM_SPACES - 1)
            {
                Factory.Instance.PlaySound(BounceSound, Pitch.GetRandomPitch());
            }
            mover.Offset = Vector2.zero;
            mover.LinearSpeed /= 2;
        }
        else if (BounceBack)
        {
            BounceBack = false;
            mover.MovementType = MovementType.Lerp;
            CheckForChain();
        }
    }

    public void Shatter(int delayMultiplier, bool sound = true)
    {
        Shattering = true;
        ShatterCountdown = delayMultiplier * BreakDelayAdd;

        pitchMultiplier = Mathf.Pow(9f / 8, delayMultiplier);
        mover.Locked = true;

        OnBreak?.Invoke();
        shatterSound = sound;

        Destroy(chainIndicator);
    }

    public void CheckForChain(bool backwards = false)
    {
        List<Orb> chainOrbs = new List<Orb> {this};

        var pos = mover.Position;
        if (backwards)
        {
            for (int i = pos.Position - 1; i >= 0; i--)
            {
                var mover = BoardController.Instance.GetMover(new RadialPosition(pos.Lane, i));
                if (mover != null && mover.TryGetComponent<Orb>(out var orb) && orb.Type == Type)
                {
                    chainOrbs.Add(orb);
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            for (int i = pos.Position + 1; i < BoardController.NUM_SPACES - 1; i++)
            {
                var mover = BoardController.Instance.GetMover(new RadialPosition(pos.Lane, i));
                if (mover != null && mover.TryGetComponent<Orb>(out var orb) && orb.Type == Type)
                {
                    chainOrbs.Add(orb);
                }
                else
                {
                    break;
                }
            }
        }

        if (chainOrbs.Count >= 3)
        {
            for (int i = 0; i < chainOrbs.Count; i++)
            {
                if (backwards)
                {
                    chainOrbs[i].AddChain(i == chainOrbs.Count - 1, i == 0);
                }
                else
                {
                    chainOrbs[i].AddChain(i == 0,i == chainOrbs.Count - 1);
                }
            }
        }

        
    }

    public void AddChain(bool cap, bool endCap)
    {
        if (chainIndicator != null)
        {
            Destroy(chainIndicator);
        }

        Quaternion rotation = Quaternion.Euler(0, 0, mover.Position.Lane / 8f * 360f + (!endCap ? 180 : 0) - 90);
        if (cap || endCap)
        {
            chainIndicator = Instantiate(CapPrefab, transform);
        }
        else
        {
            chainIndicator = Instantiate(SidePrefab, transform);
        }
        chainIndicator.transform.rotation = rotation;
        chainIndicator.GetComponent<SpriteRenderer>().color = Factory.Instance.OrbColors[Type];
    }

    public void OnMove()
    {
        CheckForChain(true);
    }
}
