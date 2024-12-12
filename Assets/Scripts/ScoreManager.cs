using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public int bestScore = 0;

    public void IncreaseScore(int points) { score += points; }
}
