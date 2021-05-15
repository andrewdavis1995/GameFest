using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SelectionState { PickingFirst, PickingSecond, Resetting }

public class PunchlineBlingController : MonoBehaviour
{
    // Display elements
    public Transform PlayerPrefab;      // The prefab to create
    public TextMesh[] NoteBookTexts;    // The text meshes used to display cards
    public Sprite[] CardBacks;          // The images to use on the back of cards (Setup then punchline)

    // config
    public Vector2[] StartPositions;    // Where the players should spawn
    public Vector2 ResultScreenPosition;

    // links to other scripts
    JokeManager _jokeManager;
    CardScript[] _cards;
    PunchlineBlingInputHandler[] _players;

    // member variables
    SelectionState _state = SelectionState.PickingFirst;
    CardScript[] _selectedCards = new CardScript[2];
    int _activePlayerIndex = 0;

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
    }

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
                SetCard_(card, 0);
                _state = SelectionState.PickingSecond;
                break;
            case SelectionState.PickingSecond:
                SetCard_(card, 1);
                _state = SelectionState.Resetting;
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
        if (_selectedCards[0].GetJoke() == _selectedCards[1].GetJoke())
        {
            // TODO: if correct, award points
            foreach (var card in _selectedCards)
                card.gameObject.SetActive(false);

            _players[_activePlayerIndex].JokeEarned(_selectedCards.First().GetJoke());

            // TODO: Add to UI

            // check for remaining cards
            var remaining = _cards.Count(c => c.gameObject.activeInHierarchy);

            // if none remaining, end game
            if (remaining == 0)
            {
                StartCoroutine(GoToEndScene());
            }
        }
        else
        {
            // if wrong, flip cards back
            foreach (var card in _selectedCards)
                card.FlipBack();

            // set next player to active
            _activePlayerIndex++;
            if (_activePlayerIndex >= _players.Length) _activePlayerIndex = 0;
        }

        // set the active player
        _players[_activePlayerIndex].ActivePlayer(true);

        //reset texts
        foreach (var txt in NoteBookTexts)
            txt.text = "";


        // back to the first one
        _state = SelectionState.PickingFirst;
    }

    /// <summary>
    /// Ends the game and moves to the result
    /// </summary>
    /// <returns></returns>
    private IEnumerator GoToEndScene()
    {
        // TODO: show a transition
        yield return new WaitForSeconds(1);
        Camera.main.transform.localPosition = ResultScreenPosition;
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
