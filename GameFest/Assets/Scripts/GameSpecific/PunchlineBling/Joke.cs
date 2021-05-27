/// <summary>
/// Class to be used to store jokes that can be used in "Punchline Bling"
/// </summary>
public class Joke
{
    public string Setup     { get; private set; }
    public string Punchline { get; private set; }
    public int JokeIndex    { get; private set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="setup">The first part of the joke</param>
    /// <param name="punchline">The ending of the joke</param>
    /// <param name="index">An ID that will allow setup and punchline to be linked</param>
    public Joke(string setup, string punchline, int index)
    {
        Setup = setup;
        Punchline = punchline;
        JokeIndex = index;
    }
}
