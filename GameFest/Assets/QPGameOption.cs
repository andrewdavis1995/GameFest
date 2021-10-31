using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum AdditionalMenuOption { None, Exit, Statistics }

/// <summary>
/// Control for showing a game option
/// </summary>
public class QPGameOption : MonoBehaviour
{
    public Scene Game;
    public Sprite BackgroundImage;
    public Sprite LogoImage;
    public RenderTexture Video;
    public VideoClip Video_Clip;
    public Image OptionBackground;
    public Text TxtName;
    public GameObject CrossIcon;
    public AdditionalMenuOption AdditionalOption;

    public int MinimumPlayers;
    public bool RequiresDualshock;

    Color BACKGROUND_COLOUR_SELECTED = new Color(0.0862745098f, 0.270588235f, 0.443137255f);

    /// <summary>
    /// When the control is deselected
    /// </summary>
    internal void Deselected()
    {
        TxtName.color = new Color(0, 0, 0);
        OptionBackground.color = new Color(0, 0, 0, 0);
        CrossIcon.SetActive(false);
    }

    /// <summary>
    /// When the control is selected
    /// </summary>
    internal void Selected()
    {
        TxtName.color = new Color(1, 1, 1);
        OptionBackground.color = BACKGROUND_COLOUR_SELECTED;
        CrossIcon.SetActive(true);
    }
}
