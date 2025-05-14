using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    public int maxScores = 5; // Store top 10 scores by default
                               // Add a new score, returns true if it's a high score
    public bool AddScore(string levelName, int score)
    {
        // Get existing scores for this level
        List<int> scores = GetHighScores(levelName);

        // Check if this is a high score
        bool isHighScore = false;
        if (scores.Count < maxScores || score > scores[scores.Count - 1])
        {
            isHighScore = true;

            // Add the new score
            scores.Add(score);

            // Sort scores (highest first)
            scores.Sort((a, b) => b.CompareTo(a));

            // Keep only top scores
            if (scores.Count > maxScores)
            {
                scores.RemoveAt(scores.Count - 1);
            }

            // Save updated scores
            SaveHighScores(levelName, scores);
        }

        return isHighScore;
    }

    // Get all high scores for a level
    public List<int> GetHighScores(string levelName)
    {
        List<int> scores = new List<int>();

        // PlayerPrefs stores the count and each score separately
        int count = PlayerPrefs.GetInt(levelName + "_count", 0);

        for (int i = 0; i < count; i++)
        {
            int score = PlayerPrefs.GetInt(levelName + "_score_" + i, 0);
            scores.Add(score);
        }

        return scores;
    }

    // Save high scores for a level
    private void SaveHighScores(string levelName, List<int> scores)
    {
        // Save count
        PlayerPrefs.SetInt(levelName + "_count", scores.Count);

        // Save each score
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetInt(levelName + "_score_" + i, scores[i]);
        }

        PlayerPrefs.Save();
    }

    // Clear high scores for a level
    public void ClearHighScores(string levelName)
    {
        int count = PlayerPrefs.GetInt(levelName + "_count", 0);

        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(levelName + "_score_" + i);
        }

        PlayerPrefs.DeleteKey(levelName + "_count");
        PlayerPrefs.Save();
    }
}
