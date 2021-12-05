using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FollowBackController : MonoBehaviour
{
    public static FollowBackController Instance;

    const int TURNS_PER_PLAYER = 2;
    const float ZONE_SIZE = 1f;
    const float ZONE_GROWTH = 2f;

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

    List<FollowBackInputHandler> _players = new List<FollowBackInputHandler>();
    public Vector3[] StartPositions;
    List<int> _remainingTurns = new List<int>();
    FollowBackInputHandler _currentInfluencer;
    TimeLimit _turnLimit;
    bool _turnRunning = false;
    bool _gameActive = false;
    List<FollowBackInputHandler> _playersInZone = new List<FollowBackInputHandler>();
    NotificationContentHandler _notificationHandler = new NotificationContentHandler();

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

        StartGame_();   // TODO: move to after fade in

        // some components will not be needed
        HideUnusedElements_();
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
        _gameActive = false;

        // TODO: show polaroids
        // TODO: show results
        // TODO: return to lobby
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    void StartGame_()
    {
        _gameActive = true;

        StartCoroutine(SelectInfluencer_());
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
            var random = UnityEngine.Random.Range(30, 45);
            yield return new WaitForSeconds(random);
            NotificationAlert.SetActive(true);
        }
    }

    /// <summary>
    /// Spawns a notification every so often
    /// </summary>
    /// <returns></returns>
    private IEnumerator FollowerNotifications_()
    {
        while (_gameActive)
        {
            var random = UnityEngine.Random.Range(15, 30);
            yield return new WaitForSeconds(random);
            FollowerAlert.SetActive(true);
        }
    }

    /// <summary>
    /// Spawns player objects and initialises UI
    /// </summary>
    void SpawnPlayers_()
    {
        // TODO: replace this with lobby stuff
        _players = FindObjectsOfType<FollowBackInputHandler>().ToList();
        int index = 0;

        // loop through players
        foreach (var pl in _players)
        {
            // TODO: remove this - just temp to test spawning
            pl.SetCharacterIndex(index);

            // spawn movement object
            pl.Spawn(PlayerPrefab, StartPositions[index], pl.GetCharacterIndex(), "player name", index, new System.Guid());

            // each player gets 2 turns
            for (int i = 0; i < TURNS_PER_PLAYER; i++)
                _remainingTurns.Add(index);

            // display in UI
            PlayerUiDisplays[index].Initialise(pl);

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
            var prefix = pl.GetFollowerCountRound() < 0 ? "lost" : "stole";
            var col = pl.GetFollowerCountRound() < 0 ? "#ea1111" : "#11ea11";
            AddVidiprinterItem(pl, $" {prefix} <color={col}>{pl.GetFollowerCountRound()}</color> followers");
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
        while (_turnRunning)
        {
            // loop through all players in the zone
            foreach (var player in _playersInZone)
            {
                // don't allow influencer to go to negative numbers
                if (_currentInfluencer.GetFollowerCount() > 0)
                {
                    // move a follower from the influencer to the player
                    player.AddFollower(true);
                    _currentInfluencer.LoseFollower(true);
                    UpdatePlayerUIs(player);
                    UpdatePlayerUIs(_currentInfluencer);
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
    /// Report a notification for the specified player
    /// </summary>
    /// <param name="player">The player who has triggered the notification</param>
    public void PlayerNotification(FollowBackInputHandler player)
    {
        _notificationHandler.GetNotificationContent();
    }
}