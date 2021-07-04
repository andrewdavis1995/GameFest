public static class MessageFetcher
{
    /// <summary>
    /// Get a string for the player to say after reading all their jokes (PLB)
    /// </summary>
    /// <returns>The message for the player to read</returns>
    public static string GetEndOfJokesString()
    {
        string[] possibleResponses = new string[]
        {
            "That's all from me... Good night!",
            "That's all we've got time for - thank you!",
            "Good night everybody!",
            "I better go now - thanks for listening!",
            "Enjoy the rest of your evening - good night!",
            "Byeeeeee!"
        };
        return GetValue(possibleResponses);
    }

    /// <summary>
    /// Get a string to display before starting to read jokes (PLB)
    /// </summary>
    /// <returns>The message to display on the fader</returns>
    public static string GetLaterThatDayString()
    {
        string[] possibleResponses = new string[]
        {
            "Later that day...",
            "That evening...",
            "That night...",
            "Later that day... but not too late... like 7pm or 8pm...",
            "Much later...",
        };
        return GetValue(possibleResponses);
    }

    /// <summary>
    /// Pick a random value from the provided options
    /// </summary>
    /// <param name="options">The list of options to choose from</param>
    /// <returns>A randomly selected string from the options</returns>
    private static string GetValue(string[] options)
    {
        var iRandom = UnityEngine.Random.Range(0, options.Length);
        return options[iRandom];
    }
}