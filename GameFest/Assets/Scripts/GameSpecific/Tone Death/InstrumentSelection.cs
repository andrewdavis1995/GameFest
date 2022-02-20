using UnityEngine;

public class InstrumentSelection : MonoBehaviour
{
    public AudioSource Audio;
    public SpriteRenderer[] InZoneDisplays;
    public SpriteRenderer ColourDisplay;
    public Instrument Instrument;
    public Material Material;

    bool _set = false;

    public void PlayerEntered(int index)
    {
        if (ToneDeathController.Instance.InstrumentRunOff)
            return;

        InZoneDisplays[index].color = ColourFetcher.GetColour(index);
        Audio.Play();
    }

    public void PlayerExited(int index)
    {
        if (ToneDeathController.Instance.InstrumentRunOff)
            return;

        InZoneDisplays[index].color = new Color(0.7f, 0.7f, 0.7f);
    }

    public void Set(bool state, int index)
    {
        _set = state;
        ColourDisplay.color = _set ? ColourFetcher.GetColour(index) : new Color(0.5f, 0.5f, 0.5f);

        foreach(var d in InZoneDisplays)
        {
            d.gameObject.SetActive(!state);
        }
    }

    public bool Set()
    {
        return _set;
    }
}
