using UnityEngine;

public class Factory : MonoBehaviour
{
    // Configuration
    public Sprite[] OrbSprites;
    public Sprite[] LargeOrbSprites;
    public Sound SoundPrefab;
    public Color[] OrbColors;

    public Orb OrbPrefab;

    // Runtime
    public static Factory Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void CreateLaneOrb(int lane, int type)
    {
        var orb = Instantiate(OrbPrefab);
        orb.Type = type;
        orb.GetComponent<SpriteRenderer>().sprite = OrbSprites[type];

        var radialPos = new RadialPosition(lane, BoardController.NUM_SPACES - 1);
        var boardMover = orb.GetComponent<BoardMover>();
        BoardController.Instance.AddMover(boardMover, radialPos);

        orb.transform.position = BoardController.Instance.GetPosition(radialPos);
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

    public void PlaySound(AudioClip clip, float pitch)
    {
        var sound = Instantiate(SoundPrefab);
        sound.Clip = clip;
        sound.Pitch = pitch;
    }
}