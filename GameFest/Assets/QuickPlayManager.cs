using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Handle the logic in the quickplay menu
/// </summary>
public class QuickPlayManager : MonoBehaviour
{
    public TransitionFader Fader;
    public Text TxtDescription;
    public QPGameOption[] Games;
    public List<QPGameOption> GameList;
    public Image TvBackground;
    public Image TvLogo;
    public VideoPlayer Video;
    public RawImage VideoImage;
    public Sprite[] PlayerIcons;

    public static QuickPlayManager Instance;

    public Transform PlayerDetailHolder;
    public Transform PlayerDetailPrefab;

    private int _gameIndex = 0;
    bool _videoFading = false;
    bool _loading = false;

    /// <summary>
    /// Called once when object starts
    /// </summary>
    private void Start()
    {
        Instance = this;

        GameList = Games.ToList();
        HideUnusableGames__();

        Fader.StartFade(1, 0, null);
        SpawnPlayers_();
        GameList[_gameIndex].Selected();
        UpdateDisplay_();
        Video.loopPointReached += Video_loopPointReached;
    }

    /// <summary>
    /// Hides any games which are not suitable for the number of players or the controllers in use
    /// </summary>
    private void HideUnusableGames__()
    {
        for (int i = 0; i < GameList.Count(); i++)
        {
            // if the game is not suitable for the players/controllers are not suitable, hide it
            if ((GameList[i].MinimumPlayers > PlayerManagerScript.Instance.GetPlayerCount())
                || GameList[i].RequiresDualshock && PlayerManagerScript.Instance.GetPlayers().Any(p => !(p.GetDevice() is DualShockGamepad)))
            {
                GameList[i].gameObject.SetActive(false);
                GameList.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Spawn players in menu
    /// </summary>
    private void SpawnPlayers_()
    {
        int index = 0;

        // don't show wins stats if there is only one player
        var showWins = PlayerManagerScript.Instance.GetPlayerCount() > 1;

        // loop through all players
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // add a win to the player if the previous handler was marked as a win
            var win = player.GetComponent<GenericInputHandler>()?.IsWinner();
            if (win == true) player.AddWin();

            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(QuickPlayInputHandler));

            // needed to set up player index
            player.Spawn(null, Vector3.zero);

            // create display
            var display = Instantiate(PlayerDetailPrefab, PlayerDetailHolder);

            // update UI
            var script = display.GetComponent<QPPlayerDisplayScript>();
            script.ImgBackground.color = ColourFetcher.GetColour(index);
            script.ImgPlayer.sprite = PlayerIcons[player.GetCharacterIndex()];
            script.TxtPlayerName.text = player.GetPlayerName();
            script.TxtPlayerWins.text = player.GetWinCount() + " win" + (player.GetWinCount() == 1 ? "" : "s"); // "win" for 1 win, "wins" for the rest
            script.TxtPlayerWins.gameObject.SetActive(showWins);

            index++;
        }
    }

    /// <summary>
    /// Moves up to next game
    /// </summary>
    public void MoveGameUp()
    {
        // TODO: skip if game is not active - or convert to a list first

        if (!_loading)
        {
            // check we are not at the top
            if (_gameIndex > 0)
            {
                // update display
                GameList[_gameIndex].Deselected();
                _gameIndex--;
                UpdateDisplay_();
                GameList[_gameIndex].Selected();
            }
        }
    }

    /// <summary>
    /// Moves down to next game
    /// </summary>
    public void MoveGameDown()
    {
        if (!_loading)
        {
            // check we are not at the bottom
            if (_gameIndex < (GameList.Count() - 1))
            {
                // update display
                GameList[_gameIndex].Deselected();
                _gameIndex++;
                UpdateDisplay_();
                GameList[_gameIndex].Selected();
            }
        }
    }

    /// <summary>
    /// Loads current game
    /// </summary>
    public void LoadGame()
    {
        if (!_loading)
        {
            _loading = true;
            Fader.StartFade(0, 1, LoadCurrentGame_);
        }
    }

    /// <summary>
    /// Loads current game
    /// </summary>
    public void LoadCurrentGame_()
    {
        if ((_gameIndex < GameList.Count()) && (GameList[_gameIndex].Game != Scene.QuickPlayLobby))
        {
            PlayerManagerScript.Instance.NextScene(GameList[_gameIndex].Game);
        }
        else
        {
            switch(GameList[_gameIndex].AdditionalOption)
            {
                case AdditionalMenuOption.Exit:
                    Application.Quit();
                    break;
                case AdditionalMenuOption.Statistics:
                    PlayerManagerScript.Instance.NextScene(Scene.Statistics);
                    break;
            }
        }
    }

    /// <summary>
    /// Displays the content for the game
    /// </summary>
    void UpdateDisplay_()
    {
        // if we somehow have an invalid index, do nothing to avoid errors
        if (_gameIndex < GameList.Count())
        {
            StopAllCoroutines();
            _videoFading = false;

            var scene = GameList[_gameIndex].Game;
            TxtDescription.text = GameDescriptionScript.GetDescription(scene);

            // setup images
            if (GameList[_gameIndex].LogoImage != null)
                TvLogo.sprite = GameList[_gameIndex].LogoImage;
            if (GameList[_gameIndex].BackgroundImage != null)
                TvBackground.sprite = GameList[_gameIndex].BackgroundImage;

            // only show video if there is something to show
            if(GameList[_gameIndex].Video_Clip != null && GameList[_gameIndex].Video != null)
            {
                // setup video player
                Video.clip = GameList[_gameIndex].Video_Clip;
                Video.targetTexture = GameList[_gameIndex].Video;
                VideoImage.texture = GameList[_gameIndex].Video;

                StartCoroutine(WaitBeforeVideo());
            }
        }
    }

    /// <summary>
    /// Does a delay before showing the video
    /// </summary>
    private IEnumerator WaitBeforeVideo()
    {
        VideoImage.color = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(2f);
        if (!_videoFading)
        {
            // play the video
            Video.time = 0;
            Video.Play();
            VideoImage.gameObject.SetActive(true);
            _videoFading = true;

            // fade video in
            for (float i = 0; i < 1 && _videoFading; i += 0.01f)
            {
                VideoImage.color = new Color(1, 1, 1, i);
                yield return new WaitForSeconds(.005f);
            }
            if (_videoFading)
                VideoImage.color = new Color(1, 1, 1, 1);
        }

        // load complete
        _videoFading = false;
        VideoImage.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// Callback for the end of the video
    /// </summary>
    /// <param name="source"></param>
    private void Video_loopPointReached(VideoPlayer source)
    {
        StartCoroutine(FadeVideoOut());
    }

    /// <summary>
    /// Fades the preview video out
    /// </summary>
    private IEnumerator FadeVideoOut()
    {
        // decrease alpha of the video
        for (float i = 1; i >= 0; i -= 0.01f)
        {
            VideoImage.color = new Color(1, 1, 1, i);
            yield return new WaitForSeconds(.005f);
        }
        VideoImage.color = new Color(1, 1, 1, 0);
    }
}
