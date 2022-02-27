using UnityEngine;

public class InstrumentSelection : MonoBehaviour
{
    public AudioSource Audio;
    public SpriteRenderer[] InZoneDisplays;
    public SpriteRenderer ColourDisplay;
    public Instrument Instrument;
    public Material Material;

    bool _set = false;

    /// <summary>
    /// Initialises the instrument display
    /// </summary>
    /// <param name="numPlayers">The number of players that are active</param>
    public void Setup(int numPlayers)
    {
        // hide unused elements
        for(int i = numPlayers; i < InZoneDisplays.Length; i++)
        {
            InZoneDisplays[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Player has entered the trigger
    /// </summary>
    /// <param name="index">The player in question</param>
    public void PlayerEntered(int index)
    {
        if (ToneDeathController.Instance.InstrumentRunOff)
            return;

        // update colour
        InZoneDisplays[index].color = ColourFetcher.GetColour(index);

        // play sound
        Audio.Play();
    }

    /// <summary>
    /// Player has exited the trigger
    /// </summary>
    /// <param name="index">Index of the player in question</param>
    public void PlayerExited(int index)
    {
        // if not running off
        if (ToneDeathController.Instance.InstrumentRunOff)
            return;

        // uppdate colour
        InZoneDisplays[index].color = new Color(0.7f, 0.7f, 0.7f);
    }

    /// <summary>
    /// Sets whether the instrument has been selected
    /// </summary>
    /// <param name="state">Whether the instrument has been selected</param>
    /// <param name="index">Index of the player who is causing this set</param>
    public void Set(bool state, int index)
    {
        _set = state;
        ColourDisplay.color = _set ? ColourFetcher.GetColour(index) : new Color(0.5f, 0.5f, 0.5f);

        // update displays
        foreach(var d in InZoneDisplays)
        {
            d.gameObject.SetActive(!state);
        }
    }

    /// <summary>
    /// Checks if the instrument has been selected
    /// </summary>
    /// <returns>Whether it has been selected</returns>
    public bool Set()
    {
        return _set;
    }
}
