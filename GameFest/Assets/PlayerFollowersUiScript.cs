using UnityEngine;
using UnityEngine.UI;

public class PlayerFollowersUiScript : MonoBehaviour
{
    public Image PlayerSprite;
    public Image PlayerSpriteBg;
    public Image PlayerSpriteBgRing;
    public Text TxtPlayerName;
    public Text TxtNumFollowers;

    /// <summary>
    /// Sets the values to display
    /// </summary>
    /// <param name="pl">The player info to display</param>
    internal void Initialise(FollowBackInputHandler pl)
    {
        TxtNumFollowers.text = pl.GetFollowerCount().ToString();
        TxtPlayerName.text = "@" + pl.GetPlayerName();
        PlayerSprite.sprite = FollowBackController.Instance.CharacterSprites[pl.GetCharacterIndex()];
        PlayerSpriteBg.color = ColourFetcher.GetColour(pl.GetPlayerIndex());
        PlayerSpriteBgRing.color = ColourFetcher.GetColour(pl.GetPlayerIndex());
    }

    /// <summary>
    /// Sets the number of followers to display
    /// </summary>
    /// <param name="followers">The number of followers to display</param>
    internal void SetFollowerCount(int followers)
    {
        TxtNumFollowers.text = followers.ToString();
    }
}
