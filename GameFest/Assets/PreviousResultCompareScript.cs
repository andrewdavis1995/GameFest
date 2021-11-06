using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PreviousResultCompareScript : MonoBehaviour
{
    public Text[] TxtScoreTexts;
    public Image[] TxtScoreImages;

    public Text DateText;

    internal void Initialise(System.Collections.Generic.List<StatContent> items, string date, System.Collections.Generic.List<Guid> profileIds)
    {
        DateText.text = date;
        var maxIndex = -1;
        var maxScore = 0;

        // update texts
        for (int i = 0; i < items.Count; i++)
        {
            var playerScore = items.Where(item => item.GetPlayerId() == profileIds[i]).FirstOrDefault().GetScore();

            TxtScoreTexts[i].text = playerScore.ToString();

            // check for max score
            if(playerScore > maxScore)
            {
                maxIndex = i;
                maxScore = playerScore;
            }
        }

        // update colours
        for (int i = 0; i < items.Count; i++)
        {
            // only full colour if won
            var a = (i == maxIndex) ? 1f : 0.125f;
            var col = TxtScoreImages[i].color;

            TxtScoreImages[i].gameObject.SetActive(true);
            TxtScoreImages[i].color = new Color(col.r, col.g, col.b, a);
        }

        // hide unused
        for (int i = items.Count; i < 4; i++)
        {
            TxtScoreImages[i].gameObject.SetActive(false);
        }
    }
    // TODO: set alpha of non-winners to lower value
}
