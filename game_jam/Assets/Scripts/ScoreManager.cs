using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    
    private static ScoreManager _instance;
    private int currentScore = 0;
    
    public static ScoreManager Instance
    {
        get { return _instance; }
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        UpdateScoreDisplay();
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
        Debug.Log("Score increased by " + points + ". Total score: " + currentScore);
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
}
