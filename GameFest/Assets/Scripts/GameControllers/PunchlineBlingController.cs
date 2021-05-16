using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SelectionState { PickingFirst, PickingSecond, Resetting }

public class PunchlineBlingController : MonoBehaviour
{
    // Display elements
    public Transform PlayerPrefab;      // The prefab to create
    public TextMesh[] NoteBookTexts;    // The text meshes used to display cards
    public Sprite[] CardBacks;          // The images to use on the back of cards (Setup then punchline)

    // config
    public Vector2[] StartPositions;         // Where the players should spawn
    public Vector3 ResultScreenPosition;     // Where the camera moves to for the results
    public Vector2 ResultPlayerPosition;     // Where the players move to for the results
    public int ResultPlayerReadingPosition;  // Where the players move to for the results

    // links to other scripts
    JokeManager _jokeManager;
    CardScript[] _cards;
    PunchlineBlingInputHandler[] _players;
    TimeLimit _overallLimit;
    TimeLimit _playerLimit;

    // member variables
    SelectionState _state = SelectionState.PickingFirst;
    CardScript[] _selectedCards = new CardScript[2];
    int _activePlayerIndex = 0;
    int _resultsPlayerIndex = 0;

    // static link to self
    public static PunchlineBlingController Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // load all jokes - must be done in Start, not constructor (as Resources must be loaded after script starts)
        _jokeManager = new JokeManager();

        // create players
        SpawnPlayers_();

        // find components
        _cards = FindObjectsOfType<CardScript>();
        _players = FindObjectsOfType<PunchlineBlingInputHandler>();

        // set all players as not active, except the first
        for (int i = 0; i < _players.Length; i++)
        {
            _players[i].ActivePlayer(i == 0);
        }

        // initialise the notepad texts
        for (var i = 0; i < NoteBookTexts.Length; i++)
        {
            NoteBookTexts[i].text = "";
        }

        CreateCards_();

        // set up the timers
        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _playerLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));

        _overallLimit.Initialise(300, OverallTickCallback, PlayerTimeoutCallback);
        _playerLimit.Initialise(25, OverallTickCallback, PlayerTimeoutCallback);

        // start te game
        StartGame();
    }

    /// <summary>
    /// Start the game and start the countdown timers
    /// </summary>
    private void StartGame()
    {
        _overallLimit.StartTimer();
        _playerLimit.StartTimer();
    }

    /// <summary>
    /// The callback to display the per player time
    /// </summary>
    /// <param name="seconds">The seconds remaining</param>
    public void PlayerTickCallback(int seconds)
    {
        // TODO: Display
        Debug.Log(seconds);
    }

    /// <summary>
    /// The callback for when timeout occurs on the per player timer
    /// </summary>
    public void PlayerTimeoutCallback()
    {
        _state = SelectionState.Resetting;
        StartCoroutine(Reset());
    }

    /// <summary>
    /// The callback to display the overall time
    /// </summary>
    /// <param name="seconds">The seconds remaining</param>
    public void OverallTickCallback(int seconds)
    {
        // TODO: Display
        Debug.Log(seconds);
    }

    /// <summary>
    /// The callback for when timeout occurs on the overall timer
    /// </summary>
    public void OverallTimeoutCallback()
    {
        StartCoroutine(GoToEndScene());
    }

    /// <summary>
    /// Gets the current state of the joke selection
    /// </summary>
    /// <returns>The state</returns>
    public SelectionState GetState()
    {
        return _state;
    }

    /// <summary>
    /// Set up the cards with a joke
    /// </summary>
    private void CreateCards_()
    {
        // get a list of jokes to use
        var jokes = _jokeManager.GetRandomisedJokeList(_cards.Length / 2);

        // get a list of indexes from 0 to the number of cards
        var remainingIndexes = new List<int>();
        for (int i = 0; i < _cards.Length; i++)
            remainingIndexes.Add(i);

        var jokeIndex = 0;

        // display the jokes
        while (remainingIndexes.Count > 1 && jokeIndex < jokes.Count)
        {
            // setup
            SetCard_(jokes[jokeIndex], false, ref remainingIndexes);

            // punchline
            SetCard_(jokes[jokeIndex], true, ref remainingIndexes);

            jokeIndex++;
        }

        // hide all remaining cards (no joke assigned)
        for (int i = 0; i < remainingIndexes.Count; i++)
        {
            _cards[remainingIndexes[i]].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Sets the content of a card
    /// </summary>
    /// <param name="joke">The joke to use</param>
    /// <param name="usePunchline">Is this the punchline? (False if it's the setup)</param>
    /// <param name="remainingIndexes">The list of indexes (of cards) that are yet to be assigned</param>
    void SetCard_(Joke joke, bool usePunchline, ref List<int> remainingIndexes)
    {
        // get a random index of card to use
        var random = UnityEngine.Random.Range(0, remainingIndexes.Count);
        var index = remainingIndexes[random];

        // display the information on the card
        _cards[index].SetJoke(joke, usePunchline);
        remainingIndexes.RemoveAt(random);
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(PunchlineBlingInputHandler));

            // create the "visual" player at the start point
            player.Spawn(PlayerPrefab, StartPositions[index++]);
        }
    }

    /// <summary>
    /// When a card is selected by the player
    /// </summary>
    /// <param name="card">The card that was selected</param>
    public void CardSelected(CardScript card)
    {
        switch (_state)
        {
            case SelectionState.PickingFirst:
                // display the first card, and move to next state
                SetCard_(card, 0);
                _state = SelectionState.PickingSecond;
                break;
            case SelectionState.PickingSecond:
                // display the second card, and move to next state
                SetCard_(card, 1);
                _state = SelectionState.Resetting;

                // swap the cards back, or remove them
                StartCoroutine(Reset());
                break;
        }
    }

    /// <summary>
    /// Pauses to display the joke, then resets card
    /// </summary>
    private IEnumerator Reset()
    {
        var activePlayer = _players.Where(p => p.ActivePlayer());

        // no player is active at this point
        foreach (var player in _players)
            player.ActivePlayer(false);

        yield return new WaitForSeconds(2);

        // if the answer is correct...
        if (_selectedCards[0] != null && _selectedCards[1] != null && _selectedCards[0].GetJoke() == _selectedCards[1].GetJoke())
        {
            // TODO: if correct, award points
            foreach (var card in _selectedCards)
                card.gameObject.SetActive(false);

            _players[_activePlayerIndex].JokeEarned(_selectedCards.First().GetJoke());

            // TODO: Add to UI

            // if none remaining, end game
            if (!CardsRemaining_())
            {
                StartCoroutine(GoToEndScene());
            }
        }
        else
        {
            // if wrong, flip cards back
            foreach (var card in _selectedCards)
                card?.FlipBack();

            // set next player to active
            _activePlayerIndex++;
            if (_activePlayerIndex >= _players.Length) _activePlayerIndex = 0;
        }

        // clear out the selection
        for (int i = 0; i < _selectedCards.Length; i++)
            _selectedCards[i] = null;

        // set the active player
        _players[_activePlayerIndex].ActivePlayer(true);

        //reset texts
        foreach (var txt in NoteBookTexts)
            txt.text = "";

        // back to the first one
        _state = SelectionState.PickingFirst;

        if (CardsRemaining_())
        {
            // restart the player countdown
            _playerLimit.StartTimer();
        }
    }

    /// <summary>
    /// Looks for remaining cards
    /// </summary>
    /// <returns>Whether there are any cards left to clear</returns>
    private bool CardsRemaining_()
    {
        // check for remaining cards
        var remaining = _cards.Count(c => c.gameObject.activeInHierarchy);
        return remaining > 0;
    }

    /// <summary>
    /// Ends the game and moves to the result
    /// </summary>
    /// <returns></returns>
    private IEnumerator GoToEndScene()
    {
        // stop timeouts
        _overallLimit.Abort();
        _playerLimit.Abort();

        // TODO: show a transition
        yield return new WaitForSeconds(1);

        // move to the end position
        SetEndPositions_();

        // tell the jokes
        StartResults_();
    }

    /// <summary>
    /// Makes each player walk on in turn, read their jokes out, then walk off
    /// </summary>
    private void StartResults_()
    {
        _players[_resultsPlayerIndex].WalkOn(ReadJokes);
    }

    /// <summary>
    /// Starts a coroutine to read out all jokes for the current player
    /// </summary>
    void ReadJokes()
    {
        StartCoroutine(CurrentPlayerReadJokes());
    }

    /// <summary>
    /// Reads out the jokes for current player - includes pauses
    /// </summary>
    IEnumerator CurrentPlayerReadJokes()
    {
        yield return new WaitForSeconds(1);

        if(_players[_resultsPlayerIndex].GetJokes().Count > 0)
        {
            foreach(var joke in _players[_resultsPlayerIndex].GetJokes())
            {
                // TODO: Display on screen
                Debug.Log(joke.Setup);
                yield return new WaitForSeconds(2);

                // TODO: Display on screen
                Debug.Log(joke.Punchline);

                yield return new WaitForSeconds(2);

                // TODO: Clear screen/hide bubble

                // TODO: points tick up
            }
        }
        else
        {
            // no jokes for this player
            Debug.Log("I got nothing");
            yield return new WaitForSeconds(2);
        }

        // TODO: "Thank you and good night!" message

        // finished
        _players[_resultsPlayerIndex].WalkOff(NextPlayerResults);
    }

    /// <summary>
    /// Once player has walked off, move to the next player
    /// </summary>
    void NextPlayerResults()
    {
        _resultsPlayerIndex++;

        // if there is a player left, show them
        if (_resultsPlayerIndex < _players.Length)
        {
            StartResults_();
        }
        else
        {
            // when no more players, move to the central page
            SceneManager.LoadScene(1);
        }
    }

    /// <summary>
    /// Sets the position of players and camera for the results
    /// </summary>
    void SetEndPositions_()
    {
        Camera.main.transform.localPosition = ResultScreenPosition;
        Camera.main.orthographicSize = 4.5f;
        foreach (var player in _players)
            player.MoveToEnd(ResultPlayerPosition);
    }

    /// <summary>
    /// Stores and displays the selected card
    /// </summary>
    /// <param name="card">The card that was selected</param>
    /// <param name="index">The index of the card (was it first or second selected?)</param>
    void SetCard_(CardScript card, int index)
    {
        _selectedCards[index] = card;
        NoteBookTexts[index].text = TextFormatter.GetNotepadJokeString(card.IsPunchline() ? card.GetJoke().Punchline : card.GetJoke().Setup);
    }
}
