using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum SelectionState { PickingFirst, PickingSecond, Resetting }

public class PunchlineBlingController : GenericController
{
    // Display elements
    public Transform PlayerPrefab;              // The prefab to create
    public TextMesh[] NoteBookTexts;            // The text meshes used to display cards
    public Sprite[] CardBacks;                  // The images to use on the back of cards (Setup then punchline)
    public Sprite[] CardFronts;                 // The images to use on the back of cards (Setup then punchline)
    public GameObject SpeechBubble;             // Speech bubble display
    public TextMesh SpeechBubbleText;           // Speech bubble text
    public Text TxtOverallTime;                 // The text for displaying the overall time
    public Text[] TxtPlayerTimes;               // The text for displaying the round time
    public Transform[] PlayerDisplays;          // The displays for showing how many jokes each player has earned
    public GameObject PnlTotalPoints;           // Displays the score during reading the results
    public Text TxtTotalPoints;                 // Displays the score during reading the results
    public GameObject SpinWheelScreen;          // The window that appears to select next character
    public Sprite[] ActiveIcons;                // The images to be used in the active icon above players head
    public GameObject PlayerDetailUI;           // The UI that displays UI info
    public Text TxtNewPoints;                   // The "+ X" points popup
    public Sprite[] CompletionMessageSprites;   // The sprites that say "Time's up" or "Complete!"
    public GameObject CompletionMessage;        // The display of the game over message
    public TransitionFader LaterFader;          // Fader for the "later that day" message
    public TransitionFader EndFader;            // Fader for the end of game
    public Text TxtLaterMsg;                    // Displays the "later that day" message
    public Transform BlingPrefab;               // Prefab for bling

    // config
    public Vector3[] StartPositions;         // Where the players should spawn
    public Vector3 ResultScreenPosition;     // Where the camera moves to for the results
    public Vector2 ResultPlayerPosition;     // Where the players move to for the results
    public int ResultPlayerReadingPosition;  // Where the players move to for the results
    public Sprite[] CharacterIcons;          // The icons for the characters
    public Sprite[] BlingSprites;            // The images of bling

    // links to other scripts
    JokeManager _jokeManager;
    CardScript[] _cards;
    PunchlineBlingInputHandler[] _players;
    TimeLimit _overallLimit;
    TimeLimit _playerLimit;
    public SpinningWheelScript SpinWheel;

    public ResultsPageScreen ResultsScreen;

    // member variables
    SelectionState _state = SelectionState.PickingFirst;
    CardScript[] _selectedCards = new CardScript[2];
    int _activePlayerIndex = 0;
    int _resultsPlayerIndex = 0;
    int _targetScore = 0;
    int _currentDisplayScore = 0;
    bool _ended = false;
    float _endSpeed = 1f;

    // static link to self
    public static PunchlineBlingController Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // fade in
        EndFader.StartFade(1, 0, FadeInComplete);

        // load all jokes - must be done in Start, not constructor (as Resources must be loaded after script starts)
        _jokeManager = new JokeManager();

        // create players
        SpawnPlayers_();

        // find components
        _cards = FindObjectsOfType<CardScript>();
        _players = FindObjectsOfType<PunchlineBlingInputHandler>().OrderBy(p => p.GetPlayerIndex()).ToArray();

        // set all players as not active
        for (int i = 0; i < _players.Length; i++)
        {
            _players[i].ActivePlayer(false, 0);
        }

        // initialise the notepad texts
        for (var i = 0; i < NoteBookTexts.Length; i++)
        {
            NoteBookTexts[i].text = "";
        }

        // hide end UI
        PnlTotalPoints.SetActive(false);

        // configure the points display controls
        for (int i = 0; i < _players.Length; i++)
        {
            var images = PlayerDisplays[i].GetComponentsInChildren<Image>();
            images[0].color = ColourFetcher.GetColour(i);
            images[1].sprite = CharacterIcons[_players[i].GetCharacterIndex()];

            // set player name on display
            var txts = PlayerDisplays[i].GetComponentsInChildren<Text>();
            txts[0].text = _players[i].GetPlayerName();
        }

        // hide unused controls
        for (int i = _players.Length; i < PlayerDisplays.Length; i++)
        {
            PlayerDisplays[i].gameObject.SetActive(false);
        }

        CreateCards_();

        // set up the timers
        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _playerLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));

        //_overallLimit.Initialise(300, OverallTickCallback, OverallTimeoutCallback);
        _overallLimit.Initialise(30, OverallTickCallback, OverallTimeoutCallback);        // TEST ONLY
        _playerLimit.Initialise(20, PlayerTickCallback, PlayerTimeoutCallback);

        SpinWheel.Initialise(_players.ToList());

        PauseGameHandler.Instance.Initialise(_players.ToList());
    }

    /// <summary>
    /// Called once fully faded in
    /// </summary>
    private void FadeInComplete()
    {
        Debug.Log("nyah");
        PauseGameHandler.Instance.Pause(true, StartGame);
    }

    private void Update()
    {
        // only continue if there are cards remaining
        if (_ended)
        {
            // count up until the target score is met
            if (_currentDisplayScore < _targetScore)
            {
                _currentDisplayScore++;
            }
            TxtTotalPoints.text = _currentDisplayScore.ToString();
        }
    }

    /// <summary>
    /// Start the game and start the countdown timers
    /// </summary>
    private void StartGame()
    {
        _overallLimit.StartTimer();

        ShowCharacterWheel();
    }

    /// <summary>
    /// The callback to display the per player time
    /// </summary>
    /// <param name="seconds">The seconds remaining</param>
    public void PlayerTickCallback(int seconds)
    {
        if (_activePlayerIndex >= 0)
            TxtPlayerTimes[_activePlayerIndex].text = TextFormatter.GetTimeString(seconds);
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
        TxtOverallTime.text = TextFormatter.GetTimeString(seconds);
    }

    /// <summary>
    /// The callback for when timeout occurs on the overall timer
    /// </summary>
    public void OverallTimeoutCallback()
    {
        StartCoroutine(GoToEndScene(false));
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
                // call again to update sprite image
                _players[_activePlayerIndex].ActivePlayer(true, 1);
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
        // stop the player timer
        _playerLimit.Abort();

        // get the active player (before they are no longer active)
        var activePlayer = _players.Where(p => p.ActivePlayer()).FirstOrDefault();

        // no player is active at this point
        foreach (var player in _players)
            player.ActivePlayer(false, 0);

        yield return new WaitForSeconds(2);

        // if the answer is correct...
        if (_selectedCards[0] != null && _selectedCards[1] != null && _selectedCards[0].GetJoke() == _selectedCards[1].GetJoke())
        {
            // disable both cards
            foreach (var card in _selectedCards)
                card.gameObject.SetActive(false);

            // add a joke to the players list
            activePlayer?.JokeEarned(_selectedCards.First().GetJoke());

            // if none remaining, end game
            if (!CardsRemaining_())
            {
                StartCoroutine(GoToEndScene(true));
            }

            // current player can stay on
            if (activePlayer != null)
                SetActivePlayer(activePlayer.GetPlayerIndex());
            else
                ShowCharacterWheel();
        }
        else
        {
            // if wrong, flip cards back
            foreach (var card in _selectedCards)
                card?.FlipBack();

            yield return new WaitForSeconds(1);

            // next player
            ShowCharacterWheel();
        }

        // clear out the selection
        for (int i = 0; i < _selectedCards.Length; i++)
            _selectedCards[i] = null;

        //reset texts
        foreach (var txt in NoteBookTexts)
            txt.text = "";
    }

    /// <summary>
    /// Shows the character wheel, which spins for X amount of time
    /// </summary>
    void ShowCharacterWheel()
    {
        if (_ended) return;

        // hides all time displays
        for (int i = 0; i < TxtPlayerTimes.Length; i++)
        {
            TxtPlayerTimes[i].gameObject.SetActive(false);
        }

        // sets the player display
        for (int i = 0; i < PlayerDisplays.Length; i++)
        {
            var rect = PlayerDisplays[i].GetComponentInChildren<RectTransform>();
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);
        }

        // show wheel
        SpinWheelScreen.SetActive(true);
        // spin
        SpinWheel.StartSpin();
    }

    /// <summary>
    /// Sets the specified player as active
    /// </summary>
    /// <param name="index">The index of the player who is now active</param>
    public void SetActivePlayer(int index)
    {
        if (_ended) return;

        _activePlayerIndex = index;

        // back to the first one
        _state = SelectionState.PickingFirst;

        // set the active player
        _players[_activePlayerIndex].ActivePlayer(true, 0);

        // hide spinning wheel
        SpinWheelScreen.SetActive(false);

        // restart the player limit time
        if (CardsRemaining_())
        {
            // restart the player countdown
            _playerLimit.StartTimer();
        }

        // sets the player display
        for (int i = 0; i < PlayerDisplays.Length; i++)
        {
            var rect = PlayerDisplays[i].GetComponentInChildren<RectTransform>();
            rect.offsetMax = new Vector2(i == index ? 125 : 0, rect.offsetMax.y);
        }

        // sets the time displays
        for (int i = 0; i < TxtPlayerTimes.Length; i++)
        {
            TxtPlayerTimes[i].gameObject.SetActive(i == index);
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
    private IEnumerator GoToEndScene(bool complete)
    {
        // if already at end scene, don't move again
        if (!_ended)
        {
            _ended = true;

            // show the completion message
            CompletionMessage.SetActive(true);
            CompletionMessage.GetComponentsInChildren<Image>()[1].sprite = CompletionMessageSprites[complete ? 1 : 0];

            // no player is active at this point
            foreach (var player in _players)
                player.ActivePlayer(false, 0);

            // stop timeouts
            _overallLimit.Abort();
            _playerLimit.Abort();

            yield return new WaitForSeconds(3);
            CompletionMessage.SetActive(false);

            LaterFader.StartFade(0, 1, ShowLaterMsg);
        }
    }

    private void ShowLaterMsg()
    {
        StartCoroutine(ShowLaterMessage_());
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
    /// Displays a message in the speech bubble
    /// </summary>
    /// <param name="msg">The message to display</param>
    void Speak(string msg)
    {
        SpeechBubble.SetActive(true);
        StartCoroutine(Speech(msg));
    }

    /// <summary>
    /// Displays a message in the speech bubble
    /// </summary>
    /// <param name="msg">The message to display</param>
    IEnumerator Speech(string msg)
    {
        // clear text
        SpeechBubbleText.text = "";

        // get message
        var split = TextFormatter.GetBubbleJokeString(msg).Split(' ');

        yield return new WaitForSeconds(0.05f);

        // add each word, one at a time
        foreach (var txt in split)
        {
            SpeechBubbleText.text += txt + " ";
            yield return new WaitForSeconds(0.15f);
        }

        SpeechBubble.SetActive(true);
    }

    /// <summary>
    /// Hides the speech bubble
    /// </summary>
    void HideSpeech()
    {
        SpeechBubble.SetActive(false);
    }

    /// <summary>
    /// Reads out the jokes for current player - includes pauses
    /// </summary>
    IEnumerator CurrentPlayerReadJokes()
    {
        yield return new WaitForSeconds(1);

        _endSpeed = 1f;

        // reset points
        TxtTotalPoints.text = "0";
        PnlTotalPoints.SetActive(true);

        // reset status
        _targetScore = 0;
        _currentDisplayScore = 0;

        // if there are jokes to display
        if (_players[_resultsPlayerIndex].GetJokes().Count > 0)
        {
            // don't animate while telling jokes
            _players[_resultsPlayerIndex].SetAnimatorState(false);

            // loop through each joke
            foreach (var joke in _players[_resultsPlayerIndex].GetJokes())
            {
                // setup
                Speak(joke.Setup);
                yield return new WaitForSeconds(4*_endSpeed);

                // Pause
                HideSpeech();
                yield return new WaitForSeconds(1 * _endSpeed);

                // punchline
                Speak(joke.Punchline);

                // add points
                var newPoints = UnityEngine.Random.Range(90, 110);
                _players[_resultsPlayerIndex].AddPoints(newPoints);

                yield return new WaitForSeconds(2 * _endSpeed);
                _targetScore = _players[_resultsPlayerIndex].GetPoints();
                StartCoroutine(ShowNewPointsFlashUp(newPoints));
                yield return new WaitForSeconds(2 * _endSpeed);

                // Pause
                HideSpeech();
                yield return new WaitForSeconds(1 * _endSpeed);
            }
        }
        else
        {
            // no jokes for this player
            Speak("I got nothing...");
            yield return new WaitForSeconds(2 * _endSpeed);
        }

        yield return new WaitForSeconds(2 * _endSpeed);

        PnlTotalPoints.SetActive(false);

        if (_players[_resultsPlayerIndex].GetJokes().Count > 0)
        {
            // exit speech
            Speak(MessageFetcher.GetEndOfJokesString());
            yield return new WaitForSeconds(3 * _endSpeed);
        }

        // walk off
        HideSpeech();
        yield return new WaitForSeconds(1 * _endSpeed);

        // re-enable animation
        _players[_resultsPlayerIndex].SetAnimatorState(true);

        // finished
        _players[_resultsPlayerIndex].WalkOff(NextPlayerResults);
    }

    /// <summary>
    /// Increases the speed at which jokes are read
    /// </summary>
    /// <param name="index">The index of the player who requested the pause</param>
    public void SpeedUp(int index)
    {
        // only do it if the requester was the current player
        if(index == _resultsPlayerIndex && _endSpeed > 0.4f)
        {
            _endSpeed /= 1.5f;
        }
    }

    /// <summary>
    /// Shows the "+ X" message on the points sign
    /// </summary>
    private IEnumerator ShowNewPointsFlashUp(int newPoints)
    {
        // set the text
        TxtNewPoints.text = "+" + newPoints.ToString();

        // get the colour of the text
        var c = TxtNewPoints.color;

        // set to fully visible for and keep that way briefly
        TxtNewPoints.color = new Color(c.r, c.g, c.b, 1f);
        yield return new WaitForSeconds(0.4f);

        // fade the colour out
        for (float i = 1; i > 0; i -= 0.01f)
        {
            TxtNewPoints.color = new Color(c.r, c.g, c.b, i);
            yield return new WaitForSeconds(0.01f);
        }
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
            // no more players, so the game is done
            StartCoroutine(Complete_());
        }
    }

    /// <summary>
    /// Show the results window, and then return to menu
    /// </summary>
    private IEnumerator Complete_()
    {
        ResultsScreen.Setup();
        ResultsScreen.SetPlayers(_players);

        yield return new WaitForSeconds(4 + _players.Length);

        // fade out
        EndFader.StartFade(0, 1, ReturnToCentral_);
    }

    /// <summary>
    /// Moves back to the central screen
    /// </summary>
    void ReturnToCentral_()
    {
        // when no more players, move to the central page
        PlayerManagerScript.Instance.NextScene(Scene.GameCentral);
    }

    /// <summary>
    /// Sets the position of players and camera for the results
    /// </summary>
    void SetEndPositions_()
    {
        // move camera to the end screen
        Camera.main.transform.localPosition = ResultScreenPosition;
        Camera.main.orthographicSize = 4.5f;

        // move the players to the end screen
        foreach (var player in _players)
            player.MoveToEnd(ResultPlayerPosition);

        // hide UI
        PlayerDetailUI.SetActive(false);
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

    /// <summary>
    /// Can't pause once we get to the results section
    /// </summary>
    /// <returns>Whether the game can be paused at the current stage</returns>
    public override bool CanPause()
    {
        return !_ended;
    }

    IEnumerator ShowLaterMessage_()
    {
        TxtLaterMsg.text = TextFormatter.GetBubbleJokeString(MessageFetcher.GetLaterThatDayString());
        TxtLaterMsg.gameObject.SetActive(true);

        var col = TxtLaterMsg.color;
        for (float i = 0f; i < 1f; i += 0.1f)
        {
            TxtLaterMsg.color = new Color(col.r, col.g, col.b, i);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(2f);

        // move to the end position
        SetEndPositions_();

        for (float i = 1f; i >= 0f; i -= 0.1f)
        {
            TxtLaterMsg.color = new Color(col.r, col.g, col.b, i);
            yield return new WaitForSeconds(0.1f);
        }

        TxtLaterMsg.color = new Color(col.r, col.g, col.b, 0);

        yield return new WaitForSeconds(1);
        LaterFader.StartFade(1f, 0f, StartResults_);
    }
}