using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FollowBackController : GenericController
{
    public static FollowBackController Instance;

    const int TURNS_PER_PLAYER = 2;
    const float ZONE_SIZE = 1f;
    const float ZONE_GROWTH = 2f;
    const float ZONE_SIZE_POWERUP = 1.4f;
    const string WEBPAGE_URL = "https://www.popster.co.uk";
    const string WEBPAGE_URL_INCORRECT = "https://www.poopster.co.uk";

    public Transform InfluencerZone;
    public Transform PlayerPrefab;
    public Sprite[] CharacterSprites;
    public Image ImgInfluencer;
    public Image ImgInfluencerBG;
    public Text TxtTrendingMessage;
    public Text TxtRefreshing;
    public PlayerFollowersUiScript[] PlayerUiDisplays;
    public VidiprintItemScript[] VidiprinterItems;
    public GameObject TrendingPanel;
    public GameObject NotificationAlert;
    public GameObject FollowerAlert;
    public Sprite SelfieIcon;
    public Text TxtUrl;
    public Text TxtNoSelfies;
    public GameObject UrlArea;
    public GameObject WebBrowserBackground;
    public GameObject ImgPageNotFound;
    public GameObject ImgPageHome;
    public GameObject SelfiePage;
    public GameObject LoadingMessage;
    public GameObject TxtMoreSelfies;
    public Transform TrollPrefab;
    public Sprite[] DisabledImages;
    public Text TxtNumPosts;

    public TransitionFader EndFader;
    public ResultsPageScreen ResultsScreen;

    // selfies
    public SelfieDisplayScript[] SelfieDisplays;

    List<FollowBackInputHandler> _players = new List<FollowBackInputHandler>();
    public Vector3[] StartPositions;
    List<int> _remainingTurns = new List<int>();
    FollowBackInputHandler _currentInfluencer;
    TimeLimit _turnLimit;
    bool _turnRunning = false;
    bool _gameActive = false;
    bool _biggerZone = false;
    bool _smallerZone = false;
    List<FollowBackInputHandler> _playersInZone = new List<FollowBackInputHandler>();
    List<Tuple<FollowBackInputHandler, FollowBackInputHandler>> _selfies = new List<Tuple<FollowBackInputHandler, FollowBackInputHandler>>();

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // clear influencer image
        ResetSpriteImage_();

        // initialise timer for each turn
        _turnLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _turnLimit.Initialise(15, TurnTickCallback, TurnTimeoutCallback);

        // spawn players
        SpawnPlayers_();

        // some components will not be needed
        HideUnusedElements_();

        // fade in
        EndFader.GetComponentInChildren<Image>().sprite = PlayerManagerScript.Instance.GetFaderImage();
        EndFader.StartFade(1, 0, FadeInComplete);

        // initialise pause
        List<GenericInputHandler> genericPlayers = _players.ToList<GenericInputHandler>();
        PauseGameHandler.Instance.Initialise(genericPlayers);
    }

    /// <summary>
    /// Called once fully faded in
    /// </summary>
    private void FadeInComplete()
    {
        PauseGameHandler.Instance.Pause(true, ShowWebLoading);
    }

    /// <summary>
    /// Shows the web page loading
    /// </summary>
    void ShowWebLoading()
    {
        // sometimes do page not found (for a bit of fun)
        var random = UnityEngine.Random.Range(0, 10);
        if (random == 1)
            StartCoroutine(ShowUrl_(WEBPAGE_URL_INCORRECT, () => StartCoroutine(PageNotFound_())));
        else
            StartCoroutine(ShowUrl_(WEBPAGE_URL, () => StartCoroutine(PageFound_())));
    }

    /// <summary>
    /// Check if a player is in the active zone
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <returns>Whether the player is in the zone</returns>
    public bool PlayerInZone(FollowBackInputHandler player)
    {
        return _playersInZone.Any(p => p == player);
    }

    /// <summary>
    /// Hide any elements that are assigned to unused players
    /// </summary>
    /// <param id="url">The url to display in the search bar</param>
    /// <param id="nextAction">The action to do once the URL has been typed</param>
    IEnumerator ShowUrl_(string url, Action nextAction)
    {
        TxtUrl.text = "";

        for (int i = 0; i < url.Length; i++)
        {
            TxtUrl.text += url[i];
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.4f));
        }
        yield return new WaitForSeconds(1f);

        ImgPageHome.gameObject.SetActive(false);
        nextAction?.Invoke();
    }

    /// <summary>
    /// Hide any elements that are assigned to unused players
    /// </summary>
    IEnumerator PageNotFound_()
    {
        ImgPageNotFound.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);

        // show real URL
        StartCoroutine(ShowUrl_(WEBPAGE_URL, () => StartCoroutine(PageFound_())));
    }

    /// <summary>
    /// Hide any elements that are assigned to unused players
    /// </summary>
    IEnumerator PageFound_()
    {
        yield return new WaitForSeconds(1f);

        // hide background to reveal game
        WebBrowserBackground.SetActive(false);

        // hide URL bar
        UrlArea.SetActive(false);

        StartGame_();
    }

    /// <summary>
    /// Hide any elements that are assigned to unused players
    /// </summary>
    private void HideUnusedElements_()
    {
        for (int i = _players.Count; i < PlayerUiDisplays.Length; i++)
        {
            PlayerUiDisplays[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Clears the image of the selected influencer
    /// </summary>
    private void ResetSpriteImage_()
    {
        TxtTrendingMessage.text = "";
        ImgInfluencer.sprite = null;
        ImgInfluencerBG.color = Color.white;
        TxtRefreshing.text = "";
    }

    /// <summary>
    /// Callback for each second the timer ticks away
    /// </summary>
    /// <param name="time">How many seconds are left</param>
    private void TurnTickCallback(int time)
    {
        TxtRefreshing.text = "Refreshing in... " + time.ToString();
    }

    /// <summary>
    /// Call back for when the turn timer expires
    /// </summary>
    private void TurnTimeoutCallback()
    {
        // hide trending panel
        TrendingPanel.SetActive(false);

        // shrink the VIP zone
        StartCoroutine(ShrinkZone_());
    }

    /// <summary>
    /// Adds an item to a vidiprinter
    /// </summary>
    public void AddVidiprinterItem(FollowBackInputHandler player, string message)
    {
        // shift content
        for (int i = 3; i >= 1; i--)
        {
            VidiprinterItems[i].Initialise(VidiprinterItems[i - 1]);
        }

        // display new message
        VidiprinterItems[0].Initialise(player, message);
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    void EndGame_()
    {
        // don't allow players to move
        foreach (var player in _players)
        {
            player.DisableMovement();
            player.DestroyAllTrolls();
        }

        _gameActive = false;
        StartCoroutine(ShowSelfies_());
    }

    /// <summary>
    /// Shows selfies taken during the game
    /// </summary>
    IEnumerator ShowSelfies_()
    {
        // show page and loading message
        SelfiePage.SetActive(true);
        UrlArea.SetActive(true);
        LoadingMessage.SetActive(true);
        WebBrowserBackground.SetActive(true);

        yield return new WaitForSeconds(2);

        // hide loading message
        LoadingMessage.SetActive(false);

        var postfix = _selfies.Count > 1 ? "s" : "";
        TxtNumPosts.text = "<b>" + _selfies.Count + "</b> post" + postfix;

        int index = 0;
        if(_selfies.Count > 0)
        {
            // show each selfie
            foreach (var selfie in _selfies)
            {
                SelfieDisplays[index].Setup(selfie);
                index++;
                yield return new WaitForSeconds(1.2f);

                // display message if more than the maximum number
                if (index >= SelfieDisplays.Length)
                {
                    TxtMoreSelfies.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            // nothing to show
            TxtNoSelfies.gameObject.SetActive(true);
        }

        // pause to show all selfies
        yield return new WaitForSeconds(2);

        // add points for each selfie
        for (int i = 0; i < index; i++)
        {
            SelfieDisplays[i].AllocatePoints();
            yield return new WaitForSeconds(0.8f);
        }

        yield return new WaitForSeconds(3);

        // go to results page
        StartCoroutine(Complete_());
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    void StartGame_()
    {
        _gameActive = true;

        // allow players to move
        foreach (var player in _players)
        {
            player.EnableMovement();
        }

        // select first player
        StartCoroutine(SelectInfluencer_());

        // start spawning alerts
        StartCoroutine(FollowerNotifications_());
        StartCoroutine(OtherNotifications_());
    }

    /// <summary>
    /// Spawns a notification every so often
    /// </summary>
    /// <returns></returns>
    private IEnumerator OtherNotifications_()
    {
        while (_gameActive)
        {
            // wait a period of time then create a notification
            var random = UnityEngine.Random.Range(30, 45);
            yield return new WaitForSeconds(random);

            if (_gameActive)
            {
                NotificationAlert.SetActive(true);
            }

            yield return new WaitForSeconds(30);
        }
    }

    /// <summary>
    /// Generates one of three events
    /// </summary>
    public void EventNotificationTriggered()
    {
        var rand = UnityEngine.Random.Range(0, 3);
        switch (rand)
        {
            // trolls
            case 0:
                // spawn trolls
                foreach (var player in _players)
                {
                    player.TrollAttack(TrollPrefab);
                }
                AddVidiprinterItem(null, "Watch out for trolls!");
                break;
            // bigger zone
            case 1:
                _biggerZone = true;
                AddVidiprinterItem(null, "BIG ZONE will be active next turn!");
                break;
            // smaller zone
            case 2:
                _smallerZone = true;
                AddVidiprinterItem(null, "LITTLE ZONE will be active next turn!");
                break;
        }
    }

    /// <summary>
    /// Spawns a notification every so often
    /// </summary>
    private IEnumerator FollowerNotifications_()
    {
        while (_gameActive)
        {
            // wait a period of time then create a follower notification
            var random = UnityEngine.Random.Range(15, 30);
            yield return new WaitForSeconds(random);
            if (_gameActive)
                FollowerAlert.SetActive(true);
        }
    }

    /// <summary>
    /// Spawns player objects and initialises UI
    /// </summary>
    void SpawnPlayers_()
    {
        int index = 0;

        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(FollowBackInputHandler));
            var ih = player.GetComponent<FollowBackInputHandler>();
            _players.Add(ih);

            // spawn movement object
            ih.Spawn(PlayerPrefab, StartPositions[index], player.GetCharacterIndex(), player.GetPlayerName(), index, player.GetGuid());

            // each player gets 2 turns
            for (int i = 0; i < TURNS_PER_PLAYER; i++)
                _remainingTurns.Add(index);

            // display in UI
            PlayerUiDisplays[index].Initialise(ih);

            index++;
        }
    }

    /// <summary>
    /// Randomly selects an influencer from list of players who are yet to have all their turns
    /// </summary>
    private IEnumerator SelectInfluencer_()
    {
        // briefly wait
        yield return new WaitForSeconds(3);

        // clear round counts
        foreach (var pl in _players)
            pl.NewRound();

        ResetSpriteImage_();

        // show trending panel
        TrendingPanel.SetActive(true);

        // flick through each player
        for (int i = 0; i < 50; i++)
        {
            TxtTrendingMessage.text = "@" + _players[i % _players.Count].GetPlayerName();
            ImgInfluencer.sprite = CharacterSprites[_players[i % _players.Count].GetCharacterIndex()];
            ImgInfluencerBG.color = ColourFetcher.GetColour(i % _players.Count);
            yield return new WaitForSeconds(0.1f);
        }

        // ensure the list is empty 
        _playersInZone.Clear();

        // pick a random player
        var r = UnityEngine.Random.Range(0, _remainingTurns.Count);
        _currentInfluencer = _players[_remainingTurns[r]];
        _remainingTurns.RemoveAt(r);

        // display selection
        TxtTrendingMessage.text = "@" + _currentInfluencer.GetPlayerName();
        ImgInfluencer.sprite = CharacterSprites[_currentInfluencer.GetCharacterIndex()];
        ImgInfluencerBG.color = ColourFetcher.GetColour(_currentInfluencer.GetPlayerIndex());

        // show zone
        InfluencerZone.gameObject.SetActive(true);
        InfluencerZone.SetParent(_currentInfluencer.MovementObject());
        InfluencerZone.localPosition = new Vector3(0, 0, 2f);

        // increase size
        StartCoroutine(GrowZone_());

        // start timer
        _turnLimit.StartTimer();

        _turnRunning = true;

        StartCoroutine(CheckZone_());
    }

    /// <summary>
    /// Grows the VIP zone to the specified zone
    /// </summary>
    IEnumerator GrowZone_()
    {
        InfluencerZone.localScale = new Vector3(0, 0, 1f);

        // adjust size to be consistent for all players
        var parentScale = _currentInfluencer.MovementObject().localScale;
        var targetSizeX = ZONE_SIZE / parentScale.x;
        var targetSizeY = ZONE_SIZE / parentScale.y;

        // if BIGGER ZONE power up triggered/pending, increase size
        if (_biggerZone)
        {
            targetSizeX *= ZONE_SIZE_POWERUP;
            targetSizeY *= ZONE_SIZE_POWERUP;
            _biggerZone = false;
        }
        // if SMALLER ZONE power up triggered/pending, decrease size
        else if (_smallerZone)
        {
            targetSizeX /= ZONE_SIZE_POWERUP;
            targetSizeY /= ZONE_SIZE_POWERUP;
            _smallerZone = false;
        }

        // increase size until target reached
        while ((InfluencerZone.localScale.x < targetSizeX - 0.01f) && (InfluencerZone.localScale.y < targetSizeY - 0.01f))
        {
            var x = Mathf.Lerp(InfluencerZone.localScale.x, targetSizeX, Time.deltaTime * ZONE_GROWTH);
            var y = Mathf.Lerp(InfluencerZone.localScale.y, targetSizeY, Time.deltaTime * ZONE_GROWTH);

            // set size
            InfluencerZone.localScale = new Vector3(x, y, 1f);
            yield return new WaitForSeconds(0.01f);
        }

        InfluencerZone.localScale = new Vector3(targetSizeX, targetSizeY, 1f);
    }

    /// <summary>
    /// Grows the VIP zone to the specified zone
    /// </summary>
    IEnumerator ShrinkZone_()
    {
        // decrease size to 0
        while ((InfluencerZone.localScale.x > 0.1f) && (InfluencerZone.localScale.y > 0.1f))
        {
            var x = Mathf.Lerp(InfluencerZone.localScale.x, 0, Time.deltaTime * ZONE_GROWTH);
            var y = Mathf.Lerp(InfluencerZone.localScale.y, 0, Time.deltaTime * ZONE_GROWTH);

            // set size
            InfluencerZone.localScale = new Vector3(x, y, 1f);
            yield return new WaitForSeconds(0.01f);
        }

        InfluencerZone.localScale = new Vector3(0, 0, 1f);

        // hide the zone and move out the way
        InfluencerZone.SetParent(null);
        _turnRunning = false;
        InfluencerZone.gameObject.SetActive(false);

        // show how many followers each player gained this round
        foreach (var pl in _players)
        {
            // don't add a feed item if no change
            if (pl.GetFollowerCountRound() != 0)
            {
                var prefix = pl.GetFollowerCountRound() < 0 ? "lost" : "stole";
                var col = pl.GetFollowerCountRound() < 0 ? "#ea1111" : "#11ea11";
                AddVidiprinterItem(pl, $" {prefix} <color={col}>{pl.GetFollowerCountRound()}</color> followers");
            }
        }

        NextTurn_();
    }

    /// <summary>
    /// Moves to the next turn
    /// </summary>
    private void NextTurn_()
    {
        // go to next turn, or end the game if nobody left to go
        if (_remainingTurns.Count > 0)
        {
            StartCoroutine(SelectInfluencer_());
        }
        else
        {
            EndGame_();
        }
    }

    /// <summary>
    /// Checks which players are in the VIP zone, and assigns followers accordingly
    /// </summary>
    IEnumerator CheckZone_()
    {
        // stop when turn ends
        while (_turnRunning && _gameActive)
        {
            // loop through all players in the zone
            foreach (var player in _playersInZone)
            {
                // don't allow influencer to go to negative numbers
                if (_currentInfluencer.GetFollowerCount() > 0)
                {
                    // move a follower from the influencer to the player
                    if (!player.TrollsActive())
                    {
                        player.AddFollower(true);
                        _currentInfluencer.LoseFollower(true);
                        UpdatePlayerUIs(player);
                        UpdatePlayerUIs(_currentInfluencer);
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Updates the UI display of the player
    /// </summary>
    /// <param name="player">The player to update</param>
    public void UpdatePlayerUIs(FollowBackInputHandler player)
    {
        // update UIs
        PlayerUiDisplays[player.GetPlayerIndex()].SetFollowerCount(player.GetFollowerCount());
    }

    /// <summary>
    /// Called when a player enters the VIP zone
    /// </summary>
    /// <param name="player">The player that entered it</param>
    internal void PlayerEnteredZone(FollowBackInputHandler player)
    {
        // add player to the list, unless it was the active player
        if (player != _currentInfluencer)
        {
            _playersInZone.Add(player);
        }
    }

    /// <summary>
    /// Called when a player leaves the VIP zone
    /// </summary>
    /// <param name="player">The player that left it</param>
    internal void PlayerLeftZone(FollowBackInputHandler player)
    {
        _playersInZone.Remove(player);
    }

    /// <summary>
    /// Add points to the winner based on number of followers
    /// </summary>
    void AddPoints_()
    {
        foreach (var p in _players)
        {
            p.AddPoints(p.GetFollowerCount());
        }
    }

    /// <summary>
    /// Assigns bonus points to the winner
    /// </summary>
    private void AssignBonusPoints_()
    {
        // sort the players by points scored
        var ordered = _players.Where(p => p.GetPoints() > 0).OrderByDescending(p => p.GetPoints()).ToList();
        int[] winnerPoints = new int[] { 100, 35, 10 };

        // add winning score points 
        for (int i = 0; i < ordered.Count(); i++)
        {
            if (ordered[i].GetPoints() > 0)
            {
                ordered[i].AddPoints(winnerPoints[i]);
                ordered[i].SetBonusPoints(winnerPoints[i]);
            }
        }

        // set the winner
        ordered.FirstOrDefault()?.Winner();
    }

    /// <summary>
    /// Completes the game and return to object
    /// </summary>
    IEnumerator Complete_()
    {
        AddPoints_();

        // assign points for winner
        AssignBonusPoints_();
        yield return new WaitForSeconds(3f);

        // show results
        ResultsScreen.Setup();
        GenericInputHandler[] genericPlayers = _players.ToArray<GenericInputHandler>();
        ResultsScreen.SetPlayers(genericPlayers);

        // store scores
        ScoreStoreHandler.StoreResults(Scene.FollowBack, genericPlayers);

        // wait for a while to show results
        yield return new WaitForSeconds(4 + genericPlayers.Length);

        // fade out
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Stores a selfie for specified player
    /// </summary>
    /// <param name="player">The player who took the selfie</param>
    public void SelfieTaken(FollowBackInputHandler player)
    {
        _selfies.Add(new Tuple<FollowBackInputHandler, FollowBackInputHandler>(player, _currentInfluencer));
        AddVidiprinterItem(player, $" took a selfie");
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        // when no more players, move to the central page
        PlayerManagerScript.Instance.CentralScene();
    }
}
