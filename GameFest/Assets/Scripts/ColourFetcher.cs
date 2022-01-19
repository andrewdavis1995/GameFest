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
                r = 0.83f;        // red = 212, 0, 22
                g = 0f;
                b = 0.0863f;
                break;
            case 1:
                r = 0.0863f;      // blue = 22, 60, 249
                g = 0.235f;
                b = 0.976f;
                break;
            case 2:
                r = 0.0863f;      // green = 22, 187, 37
                g = 0.7333f;
                b = 0.145f;
                break;
            case 3:
                r = 0.906f;       // yellow = 231, 187, 37
                g = 0.7333f;
                b = 0.145f;
                break;
            default:
                Debug.Assert(false, "Invalid index");
                break;
        }

        // set the colour
        return new Color(r, g, b);
    }
    /// <summary>
    /// Gets the colour for the specified sauce
    /// </summary>
    public static Color GetSauceColour(int index)
    {
        float r = 0, g = 0, b = 0;

        // based on the sauce index, set the colour of the sauce
        switch (index)
        {
            case 0:
                r = 0.83f;        // ketchup
                g = 0f;
                b = 0.0863f;
                break;
            case 1:
                r = 1f;           // mustard
                g = 0.8667f;
                b = 0.2196f;
                break;
            case 2:
                r = 0.349f;      // bbq
                g = 0.10196f;
                b = 0.1019f;
                break;
            default:
                Debug.Assert(false, "Invalid index");
                break;
        }

        // set the colour
        return new Color(r, g, b);
    }
}
