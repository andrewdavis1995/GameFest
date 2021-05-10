using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PunchlineBlingController : MonoBehaviour
{
    public Transform PlayerPrefab;      // The prefab to create
    public Vector2[] StartPositions;    // Where the players should spawn

    JokeManager _jokeManager;
    CardScript[] _cards;

    // Start is called before the first frame update
    void Start()
    {
        // load all jokes - must be done in Start, not constructor (as Resources must be loaded after script starts)
        _jokeManager = new JokeManager();

        _cards = FindObjectsOfType<CardScript>();
        var jokes = _jokeManager.GetRandomisedJokeList(_cards.Length / 2);

        var remainingIndexes = new List<int>();
        for (int i = 0; i < _cards.Length; i++)
            remainingIndexes.Add(i);

        var jokeIndex = 0;

        while (remainingIndexes.Count > 1 && jokeIndex < jokes.Count)
        {
            // setup
            SetCard_(jokes[jokeIndex], false, ref remainingIndexes);

            // punchline
            SetCard_(jokes[jokeIndex], true, ref remainingIndexes);

            jokeIndex++;
        }

        // hide all remaining cards (no joke assigned)
        for(int i = 0; i < remainingIndexes.Count; i++)
        {
            _cards[remainingIndexes[i]].gameObject.SetActive(false);
        }

        SpawnPlayers_();
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
}
