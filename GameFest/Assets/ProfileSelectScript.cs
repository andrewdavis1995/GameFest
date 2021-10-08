using UnityEngine;
using UnityEngine.UI;

public class ProfileSelectScript : MonoBehaviour
{
    private PlayerProfile _profile;
    private Color _selectedColour;
    private Color _deselectedColour;

    public Image CrossImage;
    public Image BackgroundImage;
    public Text TxtProfileName;

    /// <summary>
    /// Set up the control with the relevant data
    /// </summary>
    /// <param name="profile">The profile linked to this control</param>
    /// <param name="colour">The colour to set when selected</param>
    /// <param name="deselectedColour">The colour to set when not selected</param>
    public void Initialise(PlayerProfile profile, Color colour, Color deselectedColour)
    {
        _profile = profile;
        _selectedColour = colour;
        _deselectedColour = deselectedColour;

        if (profile != null)
        {
            TxtProfileName.text = profile.GetProfileName();
        }
        else
        {
            TxtProfileName.text = "New Profile";
        }
        Deselected();
    }

    /// <summary>
    /// When the item gets selected
    /// </summary>
    public void Selected()
    {
        BackgroundImage.color = _selectedColour;
        CrossImage.gameObject.SetActive(true);
    }

    /// <summary>
    /// When the item gets deselected
    /// </summary>
    public void Deselected()
    {
        BackgroundImage.color = _deselectedColour;
        CrossImage.gameObject.SetActive(false);
    }
}
