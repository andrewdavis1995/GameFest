﻿
public static class TextFormatter
{
    const int MAX_CARD_JOKE_LENGTH = 20;
    const int MAX_NOTEPAD_JOKE_LENGTH = 24;
    const int MAX_BUBBLE_JOKE_LENGTH = 25;

    /// <summary>
    /// Used to format the string of a joke so that it fits on a card
    /// </summary>
    /// <param name="original">The string to be formatted</param>
    /// <returns>The formatted string</returns>
    public static string GetCardJokeString(string original)
    {
        return GetString(original, MAX_CARD_JOKE_LENGTH);
    }

    /// <summary>
    /// Used to format the string of a joke so that it fits on a notepad
    /// </summary>
    /// <param name="original">The string to be formatted</param>
    /// <returns>The formatted string</returns>
    public static string GetNotepadJokeString(string original)
    {
        return GetString(original, MAX_NOTEPAD_JOKE_LENGTH);
    }

    /// <summary>
    /// Used to format the string of a joke so that it fits on the speech bubble
    /// </summary>
    /// <param name="original">The string to be formatted</param>
    /// <returns>The formatted string</returns>
    public static string GetBubbleJokeString(string original)
    {
        return GetString(original, MAX_BUBBLE_JOKE_LENGTH);
    }

    /// <summary>
    /// Formats the specified string so that no line is greater than the specified length
    /// </summary>
    /// <param name="original">The string to format</param>
    /// <param name="maxLineLength">The maximum length of each line</param>
    /// <returns>The formatted string</returns>
    public static string GetString(string original, int maxLineLength)
    {
        var strOutput = "";

        // split into an array of words
        var split = original.Split(' ');

        // loop through each word
        var lineLength = 0;
        foreach (var word in split)
        {
            // if the next word would take us over the maximum size, add a new line
            if (lineLength + word.Length > maxLineLength || word.Length > maxLineLength)
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
