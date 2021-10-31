using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Logic for the display script
/// </summary>
public class PlayerStatsDisplayScript : MonoBehaviour
{
    public GameObject SelectionBackground;
    public int PlayerIndex;
    public Text GuidText;
    public Text NameText;
    public Image PlayerCircle;
    public Image CharacterImage;
    Guid _playerID;

    /// <summary>
    /// Player is selected
    /// </summary>
    public void Selected()
    {
        SelectionBackground.SetActive(true);
        GuidText.color = new Color(1, 1, 1);
        PlayerCircle.color = new Color(1, 1, 1);
    }

    /// <summary>
    /// Player is deselected
    /// </summary>
    public void Deselected()
    {
        SelectionBackground.SetActive(false);

        var colour = PlayerIndex < 4 ? ColourFetcher.GetColour(PlayerIndex) : new Color(0.392f, 0.067f, 0.745f);
        GuidText.color = colour;
        PlayerCircle.color = colour;
    }

    /// <summary>
    /// Sets the data to display
    /// </summary>
    /// <param name="player">The player info</param>
    internal void SetData(PlayerControls player)
    {
        _playerID = player.GetGuid();
        GuidText.text = player.GetGuid().ToString();
        NameText.text = player.GetPlayerName();
        CharacterImage.sprite = QuickPlayManager.Instance.PlayerIcons[player.GetCharacterIndex()];
    }

    /// <summary>
    /// The ID of the player related to this string
    /// </summary>
    /// <returns>The ID of the player</returns>
    internal Guid PlayerID()
    {
        return _playerID;
    }
}
