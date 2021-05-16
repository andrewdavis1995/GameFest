using UnityEngine;

public static class ColourFetcher
{
    /// <summary>
    /// Gets the colour for this player
    /// </summary>
    public static Color GetColour(int index)
    {
        float r = 0, g = 0, b = 0;

        // based on the player index, set the colour of the player
        switch (index)
        {
            case 0:
                r = .8f;        // red
                break;
            case 1:
                b = .8f;        // blue
                break;
            case 2:
                g = .8f;        // green
                break;
            case 3:
                r = 1;          // yellow
                g = .8f;
                break;
        }

        // set the colour
        return new Color(r, g, b);
    }
}
