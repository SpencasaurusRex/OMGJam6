﻿using UnityEngine;

public class Factory : MonoBehaviour
{
    // Configuration
    public Sprite[] OrbSprites;
    public Sprite[] LargeOrbSprites;
    public Sound SoundPrefab;
    public Color[] OrbColors;
    public Vector2[] SuperShotLaserPositions;
    public Transform SuperShotLaserPrefab;
    public Transform CampfireBlastPrefab;

    public Orb OrbPrefab;
    public SpriteRenderer OrbSpawnPrefab;

    // Runtime
    public static Factory Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public Orb CreateLaneOrb(int lane, int type, int position = BoardController.NUM_SPACES - 1)
    {
        var orb = Instantiate(OrbPrefab);
        orb.Type = type;
        orb.GetComponent<SpriteRenderer>().sprite = OrbSprites[type];

        var radialPos = new RadialPosition(lane, position);
        var boardMover = orb.GetComponent<BoardMover>();
        BoardController.Instance.AddMover(boardMover, radialPos);

        orb.transform.position = BoardController.Instance.GetPosition(radialPos);
        return orb;
    }

    public void CreateOrbSpawnAnimation(Orb orb)
    {
        var spawn = Instantiate(OrbSpawnPrefab, orb.transform.position, Quaternion.identity);
        spawn.color = OrbColors[orb.Type];
    }

    public void CreateLaunchOrb(int lane, Vector2 position, int type, int lastEmptySpace)
    {
        var orb = Instantiate(OrbPrefab);
        orb.Type = type;
        orb.GetComponent<SpriteRenderer>().sprite = OrbSprites[type];
        orb.Shoot = true;

        var radialPos = new RadialPosition(lane, lastEmptySpace);
        var boardMover = orb.GetComponent<BoardMover>();
        boardMover.MovementType = MovementType.Linear;
        BoardController.Instance.AddMover(boardMover, radialPos);
        boardMover.OnCloseEnough += orb.CloseEnough;
        boardMover.Offset = (lane % 2 == 0 ? orb.StraightBump : orb.DiagonalBump).Rotate(lane / 2 * 90f);

        orb.transform.position = position;
    }

    public void PlaySound(AudioClip clip, float pitch, float volume = 1)
    {
        var sound = Instantiate(SoundPrefab);
        sound.Clip = clip;
        sound.Pitch = pitch;
        sound.Volume = volume;
    }

    public void CreateSuperShotLaser(int lane)
    {
        Instantiate(SuperShotLaserPrefab, SuperShotLaserPositions[lane], Quaternion.Euler(0, 0, (lane - 2) * 45));
    }

    public void CreateCampfireBlast()
    {
        Instantiate(CampfireBlastPrefab);
    }
}