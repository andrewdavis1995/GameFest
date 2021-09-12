using System;
using UnityEngine;
using UnityEngine.UI;

public class OverallBeachDisplayScoreScript : MonoBehaviour
{
    public BeachScoreDisplayScript[] IndividualBreakDowns;
    public BeachScoreDisplayScript[] RoundTotalScores;
    public Text TxtTotalScore;
    public GameObject ImgActiveOverlay;

    public Text TxtPlayerName;
    public Image[] ColourImages;

    public void SetColour(string name, int playerIndex)
    {
        TxtPlayerName.text = name;
        foreach (var img in ColourImages)
            img.color = ColourFetcher.GetColour(playerIndex);
    }

    internal void UpdateTotalScore()
    {
        // add up all points
        var score = 0;
        foreach (var control in RoundTotalScores)
        {
            score += control.GetValue();
        }

        TxtTotalScore.text = score.ToString();
    }
}
