using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviousResultPlayerScript : MonoBehaviour
{
    public Text DateText;
    public Text ScoreText;
    public Text SoloText;
    public Image[] OtherPlayerImages;
    public Image BackgroundImage;

    /// <summary>
    /// Sets the display data
    /// </summary>
    /// <param name="date">The date of this game</param>
    /// <param name="score">The score the player achieved</param>
    /// <param name="otherCharacters">The character indexes of tthe other players </param>
    /// <param name="win">Did the player win</param>
    public void SetData(DateTime date, int score, List<int> otherCharacters, bool win)
    {
        // TODO: Green if won, Red if loss, Blue if solo
        if(otherCharacters.Count > 0)
        {
            if (win && (score > 0))
                BackgroundImage.color = new Color(0.05f, 0.75f, 0.05f);
            else
                BackgroundImage.color = new Color(0.75f, 0.05f, 0.05f);
        }
        else
        {
            BackgroundImage.color = new Color(0.05f, 0.1f, 0.76f);
        }

        DateText.text = date.ToString("dd/MM/yyyy");
        ScoreText.text = score.ToString();

        // only show "Solo" if there were no other players
        SoloText.gameObject.SetActive(otherCharacters.Count == 0);

        // show player images
        var index = 0;
        for (; index < otherCharacters.Count && index < OtherPlayerImages.Length; index++)
        {
            OtherPlayerImages[index].sprite = QuickPlayManager.Instance.PlayerIcons[otherCharacters[index]];
            OtherPlayerImages[index].gameObject.SetActive(true);
        }

        // hide unused player images
        for (; index < OtherPlayerImages.Length; index++)
        {
            OtherPlayerImages[index].gameObject.SetActive(false);
        }
    }
}
