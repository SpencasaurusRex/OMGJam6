using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public Player Player;
    public TextMeshProUGUI TutorialText;
    TutorialState state = TutorialState.NotStarted;
    public List<SpriteRenderer> Wobbles;
    public GameObject TutorialPanel;
    public GameObject TutorialSkip;

    public delegate void TutorialDone();
    public event TutorialDone OnTutorialDone;

    int movementStep = 0;

    int shotsLeft = 0;
    int orbsLeft = 0;
    bool movedLeft;
    bool movedRight;

    void Awake()
    {
        Player.OnShootOrb += ShootOrb;
        Player.OnShootLaser += ShootLaser;
        Player.OnMove += CheckMovementDone;
        Player.OnSwapStore += OnSwap;
        Player.OnSuperShotCharged += SetupSuperShot;
    }

    public void StartTutorial()
    {
        if (state == TutorialState.Done)
        {
            FinishTutorial();
            return;
        }

        Player.MovementEnabled = false;
        Player.OrbShotEnabled = false;
        Player.SupershotEnabled = false;
        Player.SwapEnabled = false;
        Player.LaserEnabled = true;

        shotsLeft = 1;

        state = TutorialState.LaserShot;
        Factory.Instance.CreateLaneOrb(2, 0, 3).OnBreak += Break;

        TutorialPanel.SetActive(true);
        TutorialText.text = "RIGHT CLICK TO FIRE THE LASER AND DESTROY ORBS";

        Player.SetupOrbs(false, 0);
        TutorialSkip.SetActive(true);
    }

    public void ShootLaser()
    {
        if (state == TutorialState.Done) return;
        shotsLeft--;
        if (shotsLeft == 0)
        {
            Player.LaserEnabled = false;
        }
    }

    void SetupChainShot()
    {
        if (state == TutorialState.Done) return;
        Player.LaserEnabled = true;
        shotsLeft = 2;
        for (int i = 2; i < 4; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(2, 1, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
        }
        for (int i = 4; i < 6; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(2, 2, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
            if (i == 5)
            {
                orb.OnBreak += Break;
            }
        }

        TutorialText.text = "ORBS OF THE SAME COLOR BREAK TOGETHER IF TOUCHING";
        state = TutorialState.ChainShot;
    }

    void SetupOrbShoot()
    {
        if (state == TutorialState.Done) return;
        TutorialText.text = "LEFT CLICK TO LAUNCH ORBS";
        for (int i = 4; i < 6; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(2, 0, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
            if (i == 5)
            {
                orb.OnBreak += Break;
            }
        }

        state = TutorialState.OrbShoot;
        shotsLeft = 0;
        orbsLeft = 1;
        Player.LaserEnabled = false;
        Player.OrbShotEnabled = true;
    }

    void SetupSwap()
    {
        if (state == TutorialState.Done) return;
        TutorialText.text = "PRESS E TO SWAP YOUR CURRENT ORB WITH YOUR STORED ORB";
        BoardController.Instance.TryMove(Player.GetComponent<BoardMover>(), new RadialPosition(2, 0));
        Player.GetComponent<SpriteRenderer>().sprite = Player.PlayerSprites[2];
        Player.SetupOrbs(false, 5);
        Player.StoredType = 4;
        Player.CallOnSwapStore();

        Player.LaserEnabled = false;
        Player.SwapEnabled = true;
        Player.MovementEnabled = false;
        state = TutorialState.Swapping;

        for (int i = 4; i < 6; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(2, 4, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
            if (i == 5)
            {
                orb.OnBreak += Break;
            }
        }
    }

    void SetupSuperShotSetup()
    {
        if (state == TutorialState.Done) return;
        TutorialText.text = "CHARGE YOUR SUPER LASER BY BREAKING ALL THE CHAINS";
        state = TutorialState.SuperShotSetup;

        shotsLeft = 999;
        Player.LaserEnabled = true;
        Player.MovementEnabled = true;
        Player.OrbShotEnabled = false;

        for (int i = 2; i < 6; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(7, 0, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
        }

        for (int i = 3; i < 8; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(5, 1, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
        }

        for (int i = 2; i < 6; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(3, 1, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
        }
    }

    void SetupSuperShot()
    {
        if (state != TutorialState.SuperShotSetup) return;
        state = TutorialState.SuperShot;
        TutorialText.text = "FIRE SUPER SHOT WITH SPACE TO DESTROY AN ENTIRE LANE";

        Player.SupershotEnabled = true;

        for (int i = 1; i < BoardController.NUM_SPACES; i++)
        {
            var orb = Factory.Instance.CreateLaneOrb(1, i % 6, i);
            Factory.Instance.CreateOrbSpawnAnimation(orb);
            if (i == BoardController.NUM_SPACES - 1)
            {
                orb.OnBreak += Break;
            }
        }
    }

    public void OnSwap(int newStoreType, int shootType, bool _)
    {

        if (state == TutorialState.Swapping)
        {
            Player.OrbShotEnabled = newStoreType == 5;
            orbsLeft = (newStoreType == 5) ? 1 : 0;
        }
    }

    public void Break()
    {
        if (state == TutorialState.LaserShot)
        {
            Invoke("SetupChainShot", 1);
        }
        else if (state == TutorialState.ChainShot)
        {
            Invoke("SetupOrbShoot", 1);
        }
        else if (state == TutorialState.OrbShoot)
        {
            Invoke("SetupMovement", .5f);
        }
        else if (state == TutorialState.Swapping)
        {
            Invoke("SetupSuperShotSetup", 1);
        }
        else if (state == TutorialState.SuperShot)
        {
            Invoke("FinalNote", 1);
        }
    }

    public void FinalNote()
    {
        if (state == TutorialState.Done) return;

        TutorialText.text = "YOU ARE NOW READY...";
        Invoke("FinalNote2", 4);
        state = TutorialState.FinalNote;
    }

    public void FinalNote2()
    {
        if (state == TutorialState.Done) return;
        TutorialText.text = "DON'T LET ORBS SURROUND THE FIRE. LEST IT FADE FOREVER...";
        Invoke("FinishTutorial", 6);
    }

    void SetupMovement()
    {
        if (state == TutorialState.Done) return;
        TutorialText.text = "MOVE WITH A AND D";
        Player.MovementEnabled = true;
        Player.LaserEnabled = false;
        state = TutorialState.Movement;

        UpdateWobbleColor();
    }

    void UpdateWobbleColor()
    {
        if (state != TutorialState.Movement)
        {
            foreach (var wobble in Wobbles)
            {
                wobble.color = new Color(0, 0, 0, 0);
            }
        }
        else
        {
            for (int i = 0; i < Wobbles.Count; i++)
            {
                if (movementStep == i)
                {
                    Wobbles[i].color = new Color(1, 1, 1, 1);
                }
                else
                {
                    Wobbles[i].color = new Color(0, 0, 0, 0);
                }
            }
        }
    }

    public void ShootOrb(int[] _)
    {
        if (state == TutorialState.Done) return;
        orbsLeft--;
        if (orbsLeft == 0)
        {
            Player.OrbShotEnabled = false;
        }

        if (state == TutorialState.OrbShoot)
        {
            TutorialText.text = "BREAKING CHAINS OF 3 OR MORE ORBS WILL CHARGE YOUR SUPER LASER";
            Player.LaserEnabled = true;
            shotsLeft = 1;
        }

        if (state == TutorialState.Swapping)
        {
            shotsLeft = 2;
            Player.LaserEnabled = true;
        }
    }

    void CheckMovementDone(RadialPosition to)
    {
        if (state != TutorialState.Movement) return;
        if (movementStep == 0 && to.Lane == 4)
        {
            movementStep++;
        }
        else if (movementStep == 1 && to.Lane == 7)
        {
            movementStep++;
        }
        else if (movementStep == 2 && to.Lane == 5)
        {
            state = TutorialState.Swapping;
            Invoke("SetupSwap", 1);
        }
        UpdateWobbleColor();
    }

    public void FinishTutorial()
    {
        Player.MovementEnabled = true;
        Player.OrbShotEnabled = true;
        Player.SupershotEnabled = true;
        Player.LaserEnabled = true;
        Player.SwapEnabled = true;

        TutorialPanel.SetActive(false);

        state = TutorialState.Done;

        TutorialSkip.SetActive(false);
        OnTutorialDone?.Invoke();
    }

    enum TutorialState
    {
        NotStarted,
        LaserShot,
        ChainShot,
        OrbShoot,
        Movement,
        Swapping,
        SuperShotSetup,
        SuperShot,
        FinalNote,
        Done
    }
}
