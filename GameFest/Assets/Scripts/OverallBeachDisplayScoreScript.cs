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
}
