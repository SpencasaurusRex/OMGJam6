using TMPro;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    // Configuration
    public int BaseOrbScore;
    public int ChainAddition;
    public TextMeshProUGUI ScoreText;

    // Runtime
    public static ScoreController Instance { get; private set; }
    public int CurrentScore;
    public int HighScore;

    void Awake()
    {
        Instance = this;
        ScoreText.text = "";
    }

    public void StartGame()
    {
        CurrentScore = 0;
        ScoreText.color = Color.white;
        UpdateText();
    }

    public void UpdateText()
    {
        ScoreText.text = "SCORE: " + CurrentScore;
    }

    public void EndGame()
    {
        if (HighScore < CurrentScore)
        {
            HighScore = CurrentScore;
        }

        ScoreText.color = Color.white;
        ScoreText.text = "HIGH SCORE: " + HighScore;
    }

    public void OrbBreak(int index)
    {
        CurrentScore += BaseOrbScore + ChainAddition * index;
        UpdateText();
    }
}