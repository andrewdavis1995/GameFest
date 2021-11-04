using System;
using UnityEngine;
using UnityEngine.UI;

public class PreviousResultCompareScript : MonoBehaviour
{
    public Text[] TxtScoreTexts;
    public Image[] TxtScoreImages;

    public Text DateText;

    internal void Initialise(System.Collections.Generic.List<StatContent> items, string date)
    {
        DateText.text = date;
        var maxIndex = -1;
        var maxScore = 0;

        // update texts
        for (int i = 0; i < items.Count; i++)
        {
            TxtScoreTexts[i].text = items[i].GetScore().ToString();

            if(items[i].GetScore() > maxScore)
            {
                maxIndex = i;
            }
        }

        // update colours
        for (int i = 0; i < items.Count; i++)
        {
            // only full colour if won
            var a = (i == maxIndex) ? 1f : 0.25f;
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
