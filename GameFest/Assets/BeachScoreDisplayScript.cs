using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeachScoreDisplayScript : MonoBehaviour
{
    public GameObject imgDouble;
    public GameObject imgOut;
    public GameObject imgCancelled;
    public Text TxtValue;

    int _value = 0;

    public void SetValue(int value)
    {
        _value = value;
        TxtValue.text = value.ToString();
    }

    public int GetValue()
    {
        return _value;
    }

    public void ResetControl()
    {
        TxtValue.text = "-";
        imgCancelled.SetActive(false);
        imgDouble.SetActive(false);
        imgOut.SetActive(false);
        _value = 0;
    }

    public void Cancelled()
    {
        TxtValue.text = "";
        imgCancelled.SetActive(true);
        _value = 0;
    }

    public void OutOfBounds()
    {
        TxtValue.text = "";
        imgOut.SetActive(true);
        _value = 0;
    }

    public void Double()
    {
        _value = _value > 0 ? _value : 0;
        imgDouble.SetActive(true);
    }
}
