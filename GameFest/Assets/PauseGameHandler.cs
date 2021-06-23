using UnityEngine;

public class PauseGameHandler : MonoBehaviour
{// status variables
    bool _isPaused = false;
    bool _tutorialComplete = false;
    int _pageIndex = 0;

    // Unity configuration
    public GameObject PauseScreen;
    public GameObject PauseMessage;
    public GameObject ContinueButton;
    public UIFormatter Formatter;

    // static instance of itself
    public static PauseGameHandler Instance;


    // called when the script is created
    void Start()
    {
        // store static instance of this object
        Instance = this;
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

    void ConfigureAppearance_()
    {
        // TODO: Set the colours, fonts and images to match Formatter
    }

    /// <summary>
    /// Pauses the game
    /// </summary>
    /// <param name="init">Whether this is the first time it has been displayed (need to cycle through all pages)</param>
    public void Pause(bool init)
    {
        // set pause variables
        _isPaused = true;
        Time.timeScale = 0;
        _pageIndex = 0;

        // if this is the first time the page has been displayed, force the player to go through all players
        _tutorialComplete = init;

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
        for (int i = 0; i < Formatter.Pages.Length; i++)
            Formatter.Pages[i].SetActive(i == _pageIndex);
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
        }
    }

    /// <summary>
    /// Move to the next help page
    /// </summary>
    public void NextPage()
    {
        // if there are more pages, move to the next one
        if (_pageIndex < Formatter.Pages.Length - 1)
            _pageIndex++;

        // if we are at the end, we are the completion condition has been met
        if (_pageIndex >= Formatter.Pages.Length - 1)
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
}
