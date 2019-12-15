using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    // Runtime
    int volumeLevel = 3;
    bool musicOn = true;
    List<float> StartingVolumes;
    bool tutorialDone = false;
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

        Player.enabled = true;
        mainMenu = false;
        UpdateButtons();
        
        GameEventSystem.enabled = true;
        InputModule.enabled = true;
        
        Tutorial.StartTutorial();
    }

    public void TutorialDone()
    {
        WaveController.enabled = true;
        MusicSource.Play();
    }

    public void GoToMainMenu()
    {
        MainMenu.gameObject.SetActive(true);
        MusicSource.Stop();
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

        GameEventSystem.enabled = false;
        InputModule.enabled = false;
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
        VolumeImage.sprite = VolumeSprites[volumeLevel + (mainMenu ? 4 : 0)];
        MusicImage.sprite = MusicSprites[(musicOn ? 0 : 1) + (mainMenu ? 2 : 0)];
    }
}