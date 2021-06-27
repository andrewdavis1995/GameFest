using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseGameHandler : MonoBehaviour
{
    // status variables
    bool _isPaused = false;
    bool _tutorialComplete = false;
    int _pageIndex = 0;

    // Unity configuration
    public GameObject PauseScreen;
    public GameObject PauseMessage;
    public GameObject ContinueButton;
    public UIFormatter Formatter;
    public GameObject[] PausePopups;
    public GenericController ActiveGameController;
    public GameObject[] Pages;

    // Formattable objects
    public Image PauseBackground;
    public Image PauseMenu;
    public Text[] PauseTexts;
    public Text[] OutsidePanelTexts;
    public Image[] OutsidePanelImages;

    // static instance of itself
    public static PauseGameHandler Instance;

    // callback for when un-paused
    Action _unPauseCallback;

    // called when the script is created
    void Start()
    {
        // store static instance of this object
        Instance = this;
        Startup();
    }

    /// <summary>
    /// Returns the current state of the game - paused or not
    /// </summary>
    /// <returns>Whether the game is paused</returns>
    public bool IsPaused()
    {
        return _isPaused;
    }

    /// <summary>
    /// When the game starts up
    /// </summary>
    public void Startup()
    {
        Pause(true);
        ConfigureAppearance_();
    }

    /// <summary>
    /// Changes the appearance of the pause screen to match the UI Formatter
    /// </summary>
    void ConfigureAppearance_()
    {
        // background (semi-transparent) image
        PauseBackground.sprite = Formatter.WindowImage;

        // colour of the menu
        PauseMenu.color = Formatter.BackgroundColour;

        // set the colours, fonts and images
        foreach (var txt in PauseTexts)
        {
            txt.font = Formatter.MainFont;
            txt.color = Formatter.FontColour;
        }

        // set the colour and font and the texts for continue/move left & right
        foreach (var txt in OutsidePanelTexts)
        {
            txt.font = Formatter.MainFont;
            txt.color = Formatter.OutsidePanelColour;
        }

        // set the colour of the images for continue/move left & right
        foreach (var img in OutsidePanelImages)
        {
            img.color = Formatter.OutsidePanelColour;
        }
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    /// <param name="init">Whether this is the first time it has been displayed (need to cycle through all pages)</param>
    public void Pause(bool init, Action callback = null)
    {
        // set pause variables
        _isPaused = true;
        Time.timeScale = 0;
        _pageIndex = 0;
        _unPauseCallback = callback;

        // if this is the first time the page has been displayed, force the player to go through all players
        _tutorialComplete = !init;

        // show the pause screen
        PauseScreen.SetActive(true);
        PauseMessage.SetActive(!init);
        ContinueButton.SetActive(_tutorialComplete);
        ShowPage_();
    }

    /// <summary>
    /// Shows the current page on the pause screen
    /// </summary>
    public void ShowPage_()
    {
        // hide all pages, except for the current one
        for (int i = 0; i < Pages.Length; i++)
            Pages[i].SetActive(i == _pageIndex);
    }

    /// <summary>
    /// Un-pauses the game
    /// </summary>
    public void Resume()
    {
        // only allow the game to resume if the completion condition has been met
        if (_tutorialComplete)
        {
            // set the variables for un-pausing
            _isPaused = false;
            Time.timeScale = 1;

            // hide the screen
            PauseScreen.SetActive(false);

            // trigger callback, if set
            _unPauseCallback?.Invoke();
        }
    }

    /// <summary>
    /// Swaps the pause state
    /// </summary>
    public void TogglePause()
    {
        // don't pause if the game is not in a pauseable stage
        if (!ActiveGameController.CanPause()) return;

        if (_isPaused) Resume();
        else Pause(false);
    }

    /// <summary>
    /// Move to the next help page
    /// </summary>
    public void NextPage()
    {
        // if there are more pages, move to the next one
        if (_pageIndex < Pages.Length - 1)
            _pageIndex++;

        // if we are at the end, we are the completion condition has been met
        if (_pageIndex >= Pages.Length - 1)
            _tutorialComplete = true;

        // only show the continue button if the tutorial has been completed
        ContinueButton.SetActive(_tutorialComplete);

        // show the current page
        ShowPage_();
    }

    /// <summary>
    /// Move to the previous help page
    /// </summary>
    public void PreviousPage()
    {
        // as long as we aren't at the start, go back a page
        if (_pageIndex > 0)
            _pageIndex--;

        // show the current page
        ShowPage_();
    }

    /// <summary>
    /// Initialises the pause popups
    /// </summary>
    /// <param name="players">The list of players involved in the game</param>
    public void Initialise(List<PunchlineBlingInputHandler> players)
    {
        // loop through the players and configure the pause request messages
        for(int i = 1; i < players.Count; i++)
        {
            // update the colour and the name on the popup
            PausePopups[i - 1].GetComponentsInChildren<Image>()[1].color = ColourFetcher.GetColour(i);
            PausePopups[i - 1].GetComponentInChildren<Text>().text = players[i].GetPlayerName() + " requested a pause";
        }
    }
}
