using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FollowBackController : MonoBehaviour
{
    public static FollowBackController Instance;

    const int TURNS_PER_PLAYER = 2;
    const float ZONE_SIZE = 1.1f;

    public Transform InfluencerZone;
    public Transform PlayerPrefab;
    public Sprite[] CharacterSprites;
    public Image ImgInfluencer;
    public Image ImgInfluencerBG;
    public PlayerFollowersUiScript[] PlayerUiDisplays;

    List<FollowBackInputHandler> _players = new List<FollowBackInputHandler>();
    public Vector3[] StartPositions;
    List<int> _remainingTurns = new List<int>();
    FollowBackInputHandler _currentInfluencer;
    TimeLimit _turnLimit;
    List<FollowBackInputHandler> _playersInZone = new List<FollowBackInputHandler>();

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // clear influencer image
        ResetSpriteImage_();

        // initialise timer for each turn
        _turnLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _turnLimit.Initialise(20, TurnTickCallback, TurnTimeoutCallback);

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
        ImgInfluencer.sprite = null;
        ImgInfluencerBG.color = Color.white;
    }

    /// <summary>
    /// Callback for each second the timer ticks away
    /// </summary>
    /// <param name="time">How many seconds are left</param>
    private void TurnTickCallback(int time)
    {
        // TODO: show in UI
        Debug.Log(time);
    }

    /// <summary>
    /// Call back for when the turn timer expires
    /// </summary>
    private void TurnTimeoutCallback()
    {
        // hide the zone
        InfluencerZone.gameObject.SetActive(false);

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
    /// Ends the game
    /// </summary>
    void EndGame_()
    {
        // TODO: show polaroids
        // TODO: show results
        // TODO: return to lobby
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    void StartGame_()
    {
        StartCoroutine(SelectInfluencer_());
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
        ResetSpriteImage_();

        // briefly wait
        yield return new WaitForSeconds(2);

        // flick through each player
        for (int i = 0; i < 50; i++)
        {
            ImgInfluencer.sprite = CharacterSprites[_players[i % _players.Count].GetCharacterIndex()];
            ImgInfluencerBG.color = ColourFetcher.GetColour(i % _players.Count);
            yield return new WaitForSeconds(0.1f);
        }

        // pick a random player
        var r = Random.Range(0, _remainingTurns.Count);
        _currentInfluencer = _players[_remainingTurns[r]];
        _remainingTurns.RemoveAt(r);

        // display selection
        ImgInfluencer.sprite = CharacterSprites[_currentInfluencer.GetCharacterIndex()];
        ImgInfluencerBG.color = ColourFetcher.GetColour(_currentInfluencer.GetPlayerIndex());

        // show zone
        InfluencerZone.SetParent(_currentInfluencer.MovementObject());
        InfluencerZone.localPosition = new Vector3(0, 0, 2f);
        InfluencerZone.gameObject.SetActive(true);

        // adjust size to be consistent for all players
        var parentScale = _currentInfluencer.MovementObject().localScale;
        InfluencerZone.localScale = new Vector3(ZONE_SIZE / parentScale.x, ZONE_SIZE / parentScale.y, 1f);

        // start timer
        _turnLimit.StartTimer();
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
}