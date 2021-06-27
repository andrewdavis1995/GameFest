using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResultsPageScreen : MonoBehaviour
{
    // Unity configure
    public GameObject[] Controls;
    public UIFormatter Formatter;
    public Image BackgroundImage;
    public Image LogoImage;
    public Text[] Texts;

    /// <summary>
    /// Configure the display with the specified values
    /// </summary>
    public void Setup()
    {
        // show the control
        gameObject.SetActive(true);

        // hide all player info
        foreach (var go in Controls)
            go.SetActive(false);

        // set the appearance of the background image
        BackgroundImage.color = Formatter.BackgroundTransparencyColour;
        BackgroundImage.sprite = Formatter.WindowImage;

        // the appearance of the background image
        LogoImage.sprite = Formatter.LogoImage;

        // set the appearance of each text element
        foreach (var txt in Texts)
        {
            txt.font = Formatter.MainFont;
            txt.color = Formatter.FontColour;
        }
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

        // loop through all players
        for (int i = 0; i < sorted.Count; i++)
        {
            // get components
            var imgs = Controls[i].GetComponentsInChildren<Image>();
            var txts = Controls[i].GetComponentsInChildren<Text>();

            // set image appearance
            imgs[0].color = ColourFetcher.GetColour(sorted[i].GetPlayerIndex());
            imgs[1].sprite = PunchlineBlingController.Instance.CharacterIcons[sorted[i].GetCharacterIndex()];

            // get player info
            var points = sorted[i].GetPoints();
            var playerName = sorted[i].GetPlayerName();

            // display player info
            txts[0].text = playerName;
            txts[1].text = points.ToString();

            // show the display
            Controls[i].SetActive(true);

            // wait a second between each player
            yield return new WaitForSeconds(1);
        }
    }
}
