using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the overall scores, broken into base score, and bonuses
/// </summary>
public class PointDisplayBreakdown : MonoBehaviour
{
    public Text TxtOverallScore;
    public Text TxtOriginalScore;
    public Text TxtBonusScore;
    public Text TxtName;
    public Image ImgPlayerImage;
    public Image ImgBackground;

    public Sprite[] PlayerIcons;

    /// <summary>
    /// Sets the display
    /// </summary>
    /// <param name="player">The player to display</param>
    internal void SetValues(GenericInputHandler player)
    {
        TxtOverallScore.text = player.GetPoints().ToString();
        TxtBonusScore.text = "+" + player.GetBonusPoints().ToString();
        TxtOriginalScore.text = (player.GetPoints() - player.GetBonusPoints()).ToString();
        TxtName.text = player.GetPlayerName();
        ImgPlayerImage.sprite = PlayerIcons[player.GetCharacterIndex()];
        ImgBackground.color = ColourFetcher.GetColour(player.GetPlayerIndex());

        if (player.GetBonusPoints() == 0)
        {
            TxtBonusScore.text = "";
        }
    }
}
