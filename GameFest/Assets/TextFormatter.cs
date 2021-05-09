
public static class TextFormatter
{
    const int MAX_CARD_JOKE_LENGTH = 20;

    /// <summary>
    /// Used to format the string of a joke so that it fits on a card
    /// </summary>
    /// <param name="original">The string to be formatted</param>
    /// <returns>The formatted string</returns>
    public static string GetCardJokeString(string original)
    {
        var strOutput = "";

        // split into an array of words
        var split = original.Split(' ');

        // loop through each word
        var lineLength = 0;
        foreach(var word in split)
        {
            // if the next word would take us over the maximum size, add a new line
            if (lineLength + word.Length > MAX_CARD_JOKE_LENGTH || word.Length > MAX_CARD_JOKE_LENGTH)
            {
                strOutput += "\n";
                lineLength = 0;
            }
            // add the current word
            strOutput += word + " ";
            lineLength += word.Length + 1;
        }

        return strOutput;
    }

}
