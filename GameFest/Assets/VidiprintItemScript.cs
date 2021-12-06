using UnityEngine;
using UnityEngine.UI;

public class VidiprintItemScript : MonoBehaviour
{
    public Image ImgPlayerImage;
    public Image ImgBulletPoint;
    public Image ImgPlayerImageBG;
    public Text TxtDescription;

    public bool Initialised = false;

    /// <summary>
    /// Initialises the display
    /// </summary>
    /// <param name="player">The player that this relates to</param>
    /// <param name="message">The message to show</param>
    public void Initialise(FollowBackInputHandler player, string message)
    {
        TxtDescription.text = message;
        if(player != null)
        {
            ImgPlayerImageBG.gameObject.SetActive(true);
            ImgPlayerImage.sprite = FollowBackController.Instance.CharacterSprites[player.GetCharacterIndex()];
            ImgPlayerImageBG.color = ColourFetcher.GetColour(player.GetPlayerIndex());
        }
        else
        {
            ImgPlayerImageBG.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
        Initialised = true;
    }

    /// <summary>
    /// Copy
    /// </summary>
    /// <param name="copyElement">The element to copy</param>
    public void Initialise(VidiprintItemScript copyElement)
    {
        TxtDescription.text = copyElement != null ? copyElement.TxtDescription.text : "";
        ImgPlayerImage.sprite = copyElement != null ? copyElement.ImgPlayerImage.sprite : null;
        ImgPlayerImageBG.color = copyElement != null ? copyElement.ImgPlayerImageBG.color : new Color(1,1,1);
        Initialised = copyElement.Initialised;

        ImgBulletPoint.gameObject.SetActive(Initialised);
        ImgPlayerImageBG.gameObject.SetActive(copyElement.ImgPlayerImageBG.gameObject.activeInHierarchy);
        gameObject.SetActive(Initialised);
    }
}
