using UnityEngine;
using TMPro; // important for TextMeshPro

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText; // drag ScoreText here in Inspector
    public TruckAgent truckAgent;
    public float scoreDivisor = 1f;
    private float playerScore = 0f;
    private float highScore = 0f;

    void UpdateScoreDisplay()
    {
        scoreText.text = "Score: " + Mathf.FloorToInt(playerScore) + "\nHigh: " + Mathf.FloorToInt(highScore);
    }

    public void AddScore(float score)
    {
        int adjustedScore = (int)(score / scoreDivisor); //Convert x position to score.
        if (adjustedScore > playerScore) //Score only goes up.
        {
            playerScore = adjustedScore;
            if(playerScore > highScore) // set high score
            {
                highScore = playerScore;
            }
            UpdateScoreDisplay();
        }
    }

    public void GameOver()
    {
        //Debug.Log("Game Over!");
        playerScore = 0f;
        UpdateScoreDisplay();
        
        truckAgent.GameOver();
    }
}
