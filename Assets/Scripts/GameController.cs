using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // Configuration
    public Player Player;
    public WaveController WaveController;
    public GameOver GameOver;
    public MainMenu MainMenu;
    public List<AudioSource> Sources;
    public AudioSource MusicSource;
    public List<Sprite> VolumeSprites;
    public List<Sprite> MusicSprites;
    public Image VolumeImage;
    public Image MusicImage;
    public EventSystem GameEventSystem;
    public StandaloneInputModule InputModule;
    public TutorialController Tutorial;
    public GameObject MenuButton;
    public Image PauseButton;
    public GameObject PauseScreen;
    public Sprite PauseSprite;
    public Sprite PlaySprite;

    // Runtime
    bool paused;
    int volumeLevel = 3;
    bool musicOn = true;
    List<float> StartingVolumes;
    public float GlobalVolume
    {
        get
        {
            if (volumeLevel == 0) return 0;
            if (volumeLevel == 1) return 0.2f;
            if (volumeLevel == 2) return 0.5f;
            return 1f;
        }
    }
    
    public static GameController Instance { get; private set; }
    bool mainMenu = true;

    void Awake()
    {
        Instance = this;

        Player.enabled = false;
        WaveController.enabled = false;

        UpdateButtons();

        StartingVolumes = new List<float>();
        foreach (var source in Sources)
        {
            StartingVolumes.Add(source.volume);
        }

        Tutorial.OnTutorialDone += TutorialDone;
    }

    public void StartGame()
    {
        BoardController.Instance.Clear();
        
        // Turn down ambient sounds
        foreach (var source in Sources)
        {
            source.volume = source.volume - .4f;
        }

        //PauseButton.gameObject.SetActive(true);

        Player.enabled = true;
        Player.Setup();
        mainMenu = false;
        UpdateButtons();
        
        GameEventSystem.enabled = true;
        InputModule.enabled = true;
        
        Tutorial.StartTutorial();
        ScoreController.Instance.StartGame();

        LightController.Instance.LightAmount(1);
    }

    public void TutorialDone()
    {
        // Clear the board
        BoardController.Instance.Clear();
        // Reset player stats
        Player.SetupOrbs(true, 0);
        Player.Setup();

        WaveController.enabled = true;
        WaveController.Setup();
        MusicSource.Play();
        MenuButton.SetActive(true);

        ScoreController.Instance.StartGame();
    }

    public void GoToMainMenu()
    {
        ScoreController.Instance.EndGame();

        MainMenu.gameObject.SetActive(true);
        MusicSource.Stop();
        MenuButton.SetActive(false);
        PauseButton.gameObject.SetActive(false);

        MusicImage.GetComponent<EventSystem>().enabled = false;
        MusicImage.GetComponent<StandaloneInputModule>().enabled = false;
        mainMenu = true;
        UpdateButtons();
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
        MusicSource.Stop();
        GameOver.Over();
        mainMenu = true;
        UpdateButtons();

        MenuButton.SetActive(false);
        GameEventSystem.enabled = false;
        InputModule.enabled = false;
        PauseButton.gameObject.SetActive(false);

        ScoreController.Instance.EndGame();
    }

    public void ClickVolume()
    {
        volumeLevel = (volumeLevel + 1) % 4;

        for (int i = 0; i < Sources.Count; i++)
        {
            Sources[i].volume = StartingVolumes[i] * GlobalVolume;
        }
        UpdateButtons();
    }

    public void ClickMusic()
    {
        musicOn = !musicOn;

        if (musicOn)
        {
            MusicSource.volume = 0.05f;
        }
        else
        {
            MusicSource.volume = 0;
        }

        UpdateButtons();
    }

    public void UpdateButtons()
    {
        VolumeImage.sprite = VolumeSprites[volumeLevel + 4];
        MusicImage.sprite = MusicSprites[(musicOn ? 0 : 1) + 2];
        PauseButton.sprite = paused ? PlaySprite : PauseSprite;
    }

    public void PressPause()
    {
        paused = !paused;
        PauseScreen.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;

        UpdateButtons();
    }
}