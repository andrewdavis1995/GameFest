using UnityEngine;
using UnityEngine.UI;

public class HighScoreElement : MonoBehaviour
{
    public Text NameText;
    public Text ScoreText;
    public Image CharacterImage;

    /// <summary>
    /// Sets the player info
    /// </summary>
    public void SetPlayerData(string plName, int score, int characterIndex)
    {
        NameText.text = plName;
        ScoreText.text = score.ToString();
        CharacterImage.sprite = QuickPlayManager.Instance.PlayerIcons[characterIndex];
    }
}
