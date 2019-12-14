using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Player Player;
    public WaveController WaveController;
    public GameOver GameOver;
    public MainMenu MainMenu;
    public List<AudioSource> Sources;

    public static GameController Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        Player.enabled = false;
        WaveController.enabled = false;
    }

    public void StartGame()
    {
        BoardController.Instance.Clear();

        Player.enabled = true;
        WaveController.enabled = true;

        foreach (var source in Sources)
        {
            source.volume = source.volume - .4f;
        }

        GetComponent<AudioSource>().Play();
    }

    public void GoToMainMenu()
    {
        MainMenu.gameObject.SetActive(true);
        GetComponent<AudioSource>().Stop();
    }

    public void EndGame()
    {
        foreach (var source in Sources)
        {
            source.volume = source.volume + .4f;
        }
        
        GameOver.gameObject.SetActive(true);
        Player.enabled = false;
        WaveController.enabled = false;
        GetComponent<AudioSource>().Stop();
        GameOver.Over();
    }
}