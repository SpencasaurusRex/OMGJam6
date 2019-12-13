using UnityEngine;

public class GameController : MonoBehaviour
{
    public Player Player;
    public WaveController WaveController;
    public GameOver GameOver;

    public static GameController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        Player.enabled = false;
        WaveController.enabled = false;
    }

    public void StartGame()
    {
        Player.enabled = true;
        WaveController.enabled = true;
    }

    public void EndGame()
    {
        GameOver.gameObject.SetActive(true);
        Player.enabled = false;
        WaveController.enabled = false;
        GameOver.Over();
    }
}