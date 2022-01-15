using System;
using System.Collections.Generic;
using UnityEngine;

public class BurgerResultScript : MonoBehaviour
{
    public SpriteRenderer[] ElementRenderers;
    public SpriteRenderer[] TipsRenderers;
    public TextMesh TxtDescription;
    public TextMesh TxtTitle;
    public SpriteRenderer StarImage;
    public SpriteRenderer NapkinImage;

    List<BurgerComplaint> _complaints = new List<BurgerComplaint>();

    /// <summary>
    /// Sets the values of the burger
    /// </summary>
    /// <param name="customer">The complaints</param>
    /// <param name="tip">How much top was given</param>
    /// <param name="totalScore">Score achieved for this burger</param>
    /// <param name="playerIndex">Index of the player who made the burger</param>
    internal void Initialise(List<BurgerComplaint> complaints, int tip, int totalScore, int playerIndex)
    {
        Sprite bread = null;
        _complaints = complaints;

        // display tip
        for (int i = 0; i < TipsRenderers.Length; i++)
        {
            TipsRenderers[i].gameObject.SetActive((tip) >= ((i + 1) * 10));
        }

        StarImage.sprite = LicenseToGrillController.Instance.StarImages[((totalScore+19) / 20)];
        NapkinImage.color = ColourFetcher.GetColour(playerIndex);
        NapkinImage.sprite = LicenseToGrillController.Instance.NapkinImages[UnityEngine.Random.Range(0, LicenseToGrillController.Instance.NapkinImages.Length)];

        // display burger
        var index = 0;
        var message = "";

        foreach (var item in complaints)
        {
            if (item.GetSprite() != null)
            {
                // if bread, add both parts
                if (item.ErrorMessage().ToLower().Contains("bread"))
                {
                    bread = LicenseToGrillController.Instance.BreadTop[item.GetSpriteIndex()];
                }

                // add to the message
                if (message.Length > 0) message += ",\n";
                message += (message.Length > 0) ? item.ErrorMessage().ToLower() : item.ErrorMessage();

                // show the image
                ElementRenderers[index].sprite = item.GetSprite();

                ElementRenderers[index].color = item.GetColour();
                ElementRenderers[index].gameObject.SetActive(true);
                index++;
            }
        }

        // update message if flawless
        if(complaints.Count == 0)
        {
            message = MessageFetcher.GetPerfectBurgerMessage();
        }

        // format message
        TxtDescription.text = TextFormatter.GetBurgerReviewString(message);

        // show heading based on score
        TxtTitle.text = GetHeading_(totalScore);

        // add second part of bread
        if(bread)
        {
            // show the image
            ElementRenderers[index].sprite = bread;

            ElementRenderers[index].color = Color.white;
            ElementRenderers[index].gameObject.SetActive(true);
            index++;
        }

        // hide unused elements
        for (; index < ElementRenderers.Length; index++)
        {
            ElementRenderers[index].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Gets the heading to display in the review, based on the score
    /// </summary>
    /// <param name="totalScore">The score for the burger</param>
    /// <returns>The message to display</returns>
    private string GetHeading_(int totalScore)
    {
        var message = "";

        if (totalScore >= 100)
            message = "Perfection!";
        else if (totalScore >= 90)
            message = "Phenomenal!";
        else if (totalScore >= 80)
            message = "Superb!";
        else if (totalScore >= 70)
            message = "Great!";
        else if (totalScore >= 60)
            message = "I quite liked it!";
        else if (totalScore >= 50)
            message = "It was alright!";
        else if (totalScore >= 40)
            message = "Average at best!";
        else if (totalScore >= 30)
            message = "Sub-par!";
        else if (totalScore >= 20)
            message = "Appalling.";
        else if (totalScore >= 10)
            message = "Never going back.";
        else if (totalScore >= 0)
            message = "Ugh...";

        return message;
    }
}
