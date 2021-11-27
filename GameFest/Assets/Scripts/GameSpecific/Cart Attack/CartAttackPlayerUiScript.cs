using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Shows the player details in the UI (Cart Attack)
/// </summary>
public class CartAttackPlayerUiScript : MonoBehaviour
{
    const int ACCURACY_POPUP_TIME = 2;

    public Text TxtPlayerName;
    public Image PlayerColourImage;
    public Text TxtLaps;
    public Text TxtBestLap;
    public GameObject BestLap;
    public Image PowerUpIcon;
    public GameObject PowerUpIconBackground;
    public Sprite[] PowerUpIcons;

    // TODO: code for swapping positions

    /// <summary>
    /// Initialises the player display with the correct data
    /// </summary>
    /// <param id="playerName">The name of the player</param>
    /// <param id="playerIndex">The index of the player</param>
    public void Initialise(string playerName, int playerIndex)
    {
        // display player info
        TxtPlayerName.text = playerName.Substring(0, 3);
        PlayerColourImage.color = ColourFetcher.GetColour(playerIndex);

        // no laps at start
        TxtLaps.text = "0";

        // these are not seen at the start
        BestLap.SetActive(false);
        PowerUpIconBackground.SetActive(false);
        TxtBestLap.text = "00:00.000";
    }

    /// <summary>
    /// Updates the display how completed laps
    /// </summary>
    /// <param id="laps">How many laps have been completed</param>
    public void SetLapCount(int laps)
    {
        TxtLaps.text = laps.ToString();
    }

    /// <summary>
    /// Sets the content of visibility
    /// </summary>
    /// <param id="ms">Time taken for the lap</param> 
    /// <param id="thisPlayer">Whether this player was the one who set the best lap</param> 
    public void SetBestLap(bool thisPlayer, double ms)
    {
        // show/hide the display based on if this player set the lap time
        BestLap.SetActive(thisPlayer);

        // update display if it is this player
        if (thisPlayer)
        {
            // calculate time components
            int minutes = (int)(ms / 60000);
            int seconds = (int)((ms - (minutes * 60000)) / 1000);
            int milliseconds = (int)((ms - (minutes * 60000) - (seconds * 1000)));

            TxtBestLap.text = $"{minutes.ToString("00")}:{seconds.ToString("00")}.{milliseconds.ToString("000")}";
        }
    }

    /// <summary>
    /// Sets the content of visibility
    /// </summary>
    public void ShowAccuracyPopup()
    {
        StartCoroutine(ShowAccuracyPopup_());
    }

    /// <summary>
    /// Hide the power up icon
    /// </summary>
    public void HidePowerUpIcon()
    {
        PowerUpIconBackground.SetActive(false);
    }

    /// <summary>
    /// Sets the power up icon
    /// </summary>
    /// <param id="index">Index of the image to use</param> 
    public void UpdatePowerUpIcon(int index)
    {
        PowerUpIconBackground.SetActive(true);
        PowerUpIcon.sprite = PowerUpIcons[index];
    }

    /// <summary>
    /// Sets the content of visibility
    /// </summary>
    IEnumerator ShowAccuracyPopup_()
    {
        // briefly wait
        yield return new WaitForSeconds(ACCURACY_POPUP_TIME);

        // fade out
        for (float i = 1; i >= 0; i -= 0.01f)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }
}
