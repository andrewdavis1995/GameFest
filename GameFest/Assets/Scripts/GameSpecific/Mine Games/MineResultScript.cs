using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls displaying the results in Mine Games
/// </summary>
public class MineResultScript : MonoBehaviour
{
    public Text[]   BreakdownPoints;
    public Text[]   BreakdownPointsDescriptions;
    public Text     TxtTotalPoints;
    public Text     TxtPlayerName;
    public Image    ImgProfile;
    public Image    ColouredImage;
    
    /// <summary>
    /// Displays the round data for the player
    /// </summary>
    /// <param name="player"></param>
    public void SetDisplay(MineGamesInputHandler player)
    {
        // display values
        TxtPlayerName.text = player.GetPlayerName();
        TxtTotalPoints.text = player.GetRoundPoints().ToString();
        ColouredImage.color = ColourFetcher.GetColour(player.GetPlayerIndex());
        ImgProfile.sprite = MineGamesController.Instance.PlayerIcons[player.GetCharacterIndex()];

        // group items by type
        var grouped = player.GetResultList().GroupBy(p => p).ToList();
        for(int i = 0; i < grouped.Count(); i++)
        {
            var data = grouped[i].Key;
            var split = data.Split('@');

            var value = int.Parse(split[0]);
            var totalValue = value * grouped[i].Count();

            // display summaries
            BreakdownPoints[i].text = totalValue.ToString();
            BreakdownPointsDescriptions[i].text = split[1];
        }

        // hide unused controls
        for (int i = grouped.Count(); i < BreakdownPointsDescriptions.Count(); i++)
        {
            BreakdownPoints[i].text = "";
            BreakdownPointsDescriptions[i].text = "";
        }
    }
}
