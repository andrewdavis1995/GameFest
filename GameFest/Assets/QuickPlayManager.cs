using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handle the logic in the quickplay menu
/// </summary>
public class QuickPlayManager : MonoBehaviour
{
    public TransitionFader Fader;
    public Text TxtDescription;
    public QPGameOption[] Games;
    public Image TvBackground;
    public Image TvLogo;
    public RawImage TvVideo;

    public static QuickPlayManager Instance;

    private int _gameIndex = 0;

    /// <summary>
    /// Called once when object starts
    /// </summary>
    private void Start()
    {
        Instance = this;
        Fader.StartFade(1, 0, null);
        SpawnPlayers_();
        Games[_gameIndex].Selected();
        UpdateDisplay_();
    }

    /// <summary>
    /// Spawn players in menu
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(QuickPlayInputHandler));
        }
    }

    /// <summary>
    /// Moves up to next game
    /// </summary>
    public void MoveGameUp()
    {
        // check we are not at the top
        if (_gameIndex > 0)
        {
            // update display
            Games[_gameIndex].Deselected();
            _gameIndex--;
            UpdateDisplay_();
            Games[_gameIndex].Selected();
        }
    }

    /// <summary>
    /// Moves down to next game
    /// </summary>
    public void MoveGameDown()
    {
        // check we are not at the bottom
        if (_gameIndex < (Games.Length - 1))
        {
            // update display
            Games[_gameIndex].Deselected();
            _gameIndex++;
            UpdateDisplay_();
            Games[_gameIndex].Selected();
        }
    }

    /// <summary>
    /// Loads current game
    /// </summary>
    public void LoadGame()
    {
        Fader.StartFade(0, 1, LoadCurrentGame_);
    }

    /// <summary>
    /// Loads current game
    /// </summary>
    public void LoadCurrentGame_()
    {
        PlayerManagerScript.Instance.NextScene(Games[_gameIndex].Game);
    }

    /// <summary>
    /// Displays the content for the game
    /// </summary>
    void UpdateDisplay_()
    {
        var scene = Games[_gameIndex].Game;
        TxtDescription.text = GameDescriptionScript.GetDescription(scene);

        TvLogo.sprite = Games[_gameIndex].LogoImage;
        TvBackground.sprite = Games[_gameIndex].BackgroundImage;
    }
}
