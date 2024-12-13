using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public int bestScore = 0;

    public int lives = 3;

    [SerializeField] Text scoreText;
    [SerializeField] Transform livesObj;
    [SerializeField] AudioSource mistake;
    [SerializeField] AudioSource success;

    public void IncreaseScore(int points) 
    { 
        score += points;
        scoreText.text = score.ToString();
        success.Play();
    }
    public void DecreaseLives() 
    { 
        lives--;
        livesObj.GetChild(lives).gameObject.SetActive(false);
        mistake.Play();
    }

    public void UpdateBestScore()
    {
        if (score > bestScore) bestScore = score;
    }

    public void ResetAll()
    {
        score = 0;
        scoreText.text = score.ToString();

        lives = 3;
        for (int i = 0; i < lives; i++) 
        {
            livesObj.GetChild(i).gameObject.SetActive(true);
        }
    }
}
