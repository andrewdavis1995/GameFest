using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResultsPageScreen : MonoBehaviour
{
    // Unity configure
    public PointDisplayBreakdown[] Controls;
    public UIFormatter Formatter;
    public Image BackgroundImage;
    public Image LogoImage;

    /// <summary>
    /// Configure the display with the specified values
    /// </summary>
    public void Setup()
    {
        // show the control
        gameObject.SetActive(true);

        // hide all player info
        foreach (var go in Controls)
            go.gameObject.SetActive(false);

        // set the appearance of the background image
        BackgroundImage.color = Formatter.BackgroundTransparencyColour;
        BackgroundImage.sprite = Formatter.WindowImage;

        // the appearance of the background image
        LogoImage.sprite = Formatter.LogoImage;
    }

    /// <summary>
    /// Displays the players on the table
    /// </summary>
    /// <param name="players">The list of players to display</param>
    public void SetPlayers(GenericInputHandler[] players)
    {
        StartCoroutine(DisplayPlayersAtTime_(players));
    }

    /// <summary>
    /// Displays the players one at a time
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    private IEnumerator DisplayPlayersAtTime_(GenericInputHandler[] players)
    {
        // order the players based on points
        var sorted = players.OrderByDescending(p => p.GetPoints()).ToList();

        // wait a second at the start
        yield return new WaitForSeconds(1);

        // loop through all players
        for (int i = 0; i < sorted.Count; i++)
        {
            Controls[i].SetValues(sorted[i]);

            // show the display
            Controls[i].gameObject.SetActive(true);

            // wait a second between each player
            yield return new WaitForSeconds(1);
        }
    }
}
