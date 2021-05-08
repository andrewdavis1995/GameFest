using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to store and retrieve all jokes that can be used in "Punchline Bling"
/// </summary>
public class JokeManager
{
    // list of all jokes that can be selected
    List<Joke> _jokes = new List<Joke>();

    /// <summary>
    /// Constructor
    /// </summary>
    public JokeManager()
    {
        // TODO: abort the game if the text file is not loaded
        TextAsset mytxtData = (TextAsset)Resources.Load("TextFiles/JokeList");
        string txt = mytxtData.text;

        // split line-by-line
        var lines = txt.Split('\n');
        foreach (var line in lines)
        {
            // add each line to the list of jokes
            var split = line.Split('@');
            if (split.Length > 1)
                _jokes.Add(new Joke(split[0], split[1], _jokes.Count));
        }
    }

    /// <summary>
    /// Returns a random list of jokes from the collection
    /// </summary>
    /// <param name="numberOfJokes">How many jokes to fetch</param>
    /// <returns></returns>
    public List<Joke> GetRandomisedJokeList(int numberOfJokes)
    {
        var list = new List<Joke>();

        // take a copy of the jokes that can be edited (items removed from)
        var copy = new List<Joke>(_jokes);

        // loop until reached the requested number of jokes, or run out of jokes to use
        while (list.Count < numberOfJokes && copy.Count > 0)
        {
            // get a random joke
            var random = UnityEngine.Random.Range(0, copy.Count);

            // add it to the new list
            list.Add(copy[random]);

            // remove it from the copy list so it is not used again
            copy.RemoveAt(random);
        }

        return list;
    }
}
