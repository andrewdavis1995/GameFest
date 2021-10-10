using System.Collections;
using UnityEngine;
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
        Fader.StartFade(1, 0, null);
        SpawnPlayers_();
        Games[_gameIndex].Selected();
        UpdateDisplay_();
        Video.loopPointReached += Video_loopPointReached;
    }

    /// <summary>
    /// Spawn players in menu
    /// </summary>
    private void SpawnPlayers_()
    {
        int index = 0;

        // don't show wins stats if there is only one player
        var showWins = PlayerManagerScript.Instance.GetPlayers().Count > 1;

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
        if (!_loading)
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
    }

    /// <summary>
    /// Moves down to next game
    /// </summary>
    public void MoveGameDown()
    {
        if (!_loading)
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
        if ((_gameIndex < Games.Length) && (Games[_gameIndex].Game != Scene.QuickPlayLobby))
        {
            PlayerManagerScript.Instance.NextScene(Games[_gameIndex].Game);
        }
        else
        {
            Debug.Log("Leaving");
            Application.Quit();
        }
    }

    /// <summary>
    /// Displays the content for the game
    /// </summary>
    void UpdateDisplay_()
    {
        if (_gameIndex < Games.Length)
        {
            StopAllCoroutines();
            _videoFading = false;

            var scene = Games[_gameIndex].Game;
            TxtDescription.text = GameDescriptionScript.GetDescription(scene);

            TvLogo.sprite = Games[_gameIndex].LogoImage;
            TvBackground.sprite = Games[_gameIndex].BackgroundImage;

            Video.clip = Games[_gameIndex].Video_Clip;
            Video.targetTexture = Games[_gameIndex].Video;
            VideoImage.texture = Games[_gameIndex].Video;

            StartCoroutine(WaitBeforeVideo());
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
